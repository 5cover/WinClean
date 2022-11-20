using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.Input;

using Humanizer;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

using Xceed.Wpf.AvalonDock.Controls;

using Button = Scover.WinClean.Presentation.Dialogs.Button;

namespace Scover.WinClean.Presentation.Windows;

public sealed partial class MainWindow
{
    private static readonly IScriptSerializer serializer = new ScriptXmlSerializer();
    private readonly Dictionary<Category, Script?> _selectedScripts = new();

    public MainWindow()
    {
        InitializeComponent();
        Top = AppInfo.Settings.Top;
        Left = AppInfo.Settings.Left;
        Width = AppInfo.Settings.Width;
        Height = AppInfo.Settings.Height;
        WindowState = AppInfo.Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    public static RelayCommand<ScriptMetadata> CheckScriptsByMetadata { get; } = new(metadata =>
        {
            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));
            CheckScripts(s =>
            {
                ScriptMetadata scriptMetadata = metadata switch
                {
                    Category => s.Category,
                    RecommendationLevel => s.RecommendationLevel,
                    Host => s.Host,
                    Impact => s.Impact,
                    _ => throw new ArgumentException($"'{metadata.GetType()}' is an unknown script metadata type", nameof(metadata))
                };
                return scriptMetadata == metadata;
            });
        });

    public static ObservableCollection<Script> Scripts { get; } = new(App.Scripts);

    private ICollectionView ScriptCollectionView => ((CollectionViewSource)Resources["Scripts"]).View;
    private DataGrid? ScriptDataGrid => TabControlCategories.FindVisualChildren<DataGrid>().SingleOrDefault();

    /// <summary>Gets or sets the currently selected script.</summary>
    /// <remarks>Asserts that <see cref="ScriptDataGrid"/> is not null before setting the selected script.</remarks>
    private Script? SelectedScript
    {
        get => ScriptEditor.Selected;
        // Don't set ScriptEditor.Selected because it wouldn't update ScriptDataGrid.SelectedItem. Setting
        // ScriptDataGrid.SelectedItem updates ScriptEditor.Selected due to a OneWayToSource binding in ScriptDataGrid.
        set => ScriptDataGrid.AssertNotNull().SelectedItem = value;
    }

    private static void AddScript(string path)
    {
    retry:
        try
        {
            using Stream file = File.OpenRead(path);
            Script script = serializer.Deserialize(ScriptType.Custom, file);

            Script? existingScript = Scripts.FirstOrDefault(s => s.InvariantName == script.InvariantName);
            if (existingScript is null)
            {
                Scripts.Add(script);
            }
            else
            {
                using Dialog overwrite = new(Button.Yes, Button.No)
                {
                    MainIcon = TaskDialogIcon.Warning,
                    Content = WinClean.Resources.UI.Dialogs.ConfirmScriptOverwriteContent.FormatWith(existingScript.Name)
                };
                if (overwrite.ShowDialog().ClickedButton == Button.Yes)
                {
                    _ = Scripts.Remove(existingScript);
                    goto retry;
                }
            }
        }
        catch (InvalidDataException ex)
        {
            Logs.InvalidScriptData.FormatWith(Path.GetFileName(path), ex).Log(LogLevel.Error);

            using Dialog invalidScriptData =
                DialogFactory.MakeInvalidScriptDataDialog(ex, path, Button.Retry, Button.Ignore);
            if (invalidScriptData.ShowDialog().ClickedButton == Button.Retry)
            {
                goto retry;
            }
        }
        catch (Exception ex) when (ex.IsFileSystem())
        {
            using FSErrorDialog fsErrorDialog = new(ex, FSVerb.Access, new FileInfo(path), Button.Retry, Button.Ignore)
            {
                MainInstruction = WinClean.Resources.UI.Dialogs.FSErrorAddingCustomScriptMainInstruction
            };
            if (fsErrorDialog.ShowDialog().ClickedButton == Button.Retry)
            {
                goto retry;
            }
        }
    }

    private static void CheckScripts(Predicate<Script> check)
    {
        foreach (var script in Scripts)
        {
            script.IsSelected = check(script);
        }
    }

    private void RefreshScriptFilter(Category category)
    {
        // 1. Commit any pending edits and exit edit mode before setting the filter.
        _ = ScriptDataGrid?.CommitEdit(DataGridEditingUnit.Row, true);

        // 2. Set the filter to keep only the scripts with the current category.
        ScriptCollectionView.Filter = s => ((Script)s).Category.Equals(category);
        // 3. Refresh to apply changes
        ScriptCollectionView.Refresh();

        // 4. Refreshing unfocuses the DataGrid, so focus it.
        _ = ScriptDataGrid?.Focus();

        // ScriptDataGrid could be null here because this method may be called right when the window is shown, before all
        // elements have initialized.
        if (ScriptDataGrid is not null && _selectedScripts.TryGetValue(category, out var selected))
        {
            // 5. Restore the selected item.
            SelectedScript = selected;
        }
    }
}