using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

using static Scover.WinClean.Resources.UI.Dialogs;

using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;

namespace Scover.WinClean.Presentation.Windows;

public sealed partial class MainWindow
{
    private readonly Dictionary<Category, CheckableScript?> _selectedScriptsDictionary = new();

    public MainWindow(IEnumerable<Script> scripts)
    {
        Scripts = new(scripts.Select(s => new CheckableScript(s)));
        InitializeComponent();
        Top = App.Settings.Top;
        Left = App.Settings.Left;
        Width = App.Settings.Width;
        Height = App.Settings.Height;
        WindowState = App.Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;

        CheckScriptsByMetadata = new(metadata => CheckScripts(s =>
        {
            ScriptMetadata scriptMetadata = metadata switch
            {
                Category => s.Category,
                RecommendationLevel => s.RecommendationLevel,
                Host => s.Host,
                Impact => s.Impact,
                _ => throw new ArgumentException($"'{metadata?.GetType()}' is an unknown script metadata type", nameof(metadata))
            };
            return scriptMetadata == metadata;
        }));
    }

    public RelayCommand<ScriptMetadata> CheckScriptsByMetadata { get; }

    public ObservableCollection<CheckableScript> Scripts { get; }
    private DataGrid? DataGridScripts => TabControlCategories.FindVisualChildren<DataGrid>().SingleOrDefault();

    private ICollectionView ScriptCollectionView => ((CollectionViewSource)Resources["Scripts"]).View;

    /// <summary>Gets or sets the currently selected script.</summary>
    /// <remarks>Asserts that <see cref="DataGridScripts"/> is not null before setting the selected script.</remarks>
    private CheckableScript? SelectedScript
    {
        get => (CheckableScript?)ScriptEditor.Selected;
        // Don't set ScriptEditor.Selected because it wouldn't update DataGridScripts.SelectedItem. Setting
        // DataGridScripts.SelectedItem updates ScriptEditor.Selected due to a OneWayToSource binding in DataGridScripts.
        set => DataGridScripts.AssertNotNull().SelectedItem = value;
    }

    private void AddScript(string path)
    {
        bool retry = true;
        while (retry)
        {
            try
            {
                CheckableScript script = new(App.Scripts.Add(ScriptType.Custom, path));

                if (Scripts.FirstOrDefault(s => s.Equals(script)) is { } existingScript)
                {
                    throw new ScriptAlreadyExistsException(existingScript);
                }
                else
                {
                    Scripts.Add(new(script));
                    Logs.ScriptAdded.FormatWith(path, script.InvariantName).Log();
                }
                retry = false;
            }
            catch (InvalidDataException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page page = DialogPages.InvalidScriptData(e, path, new() { Button.Retry, Button.Ignore });
                retry = Button.Retry.Equals(new Dialog(page).Show());
            }
            catch (ScriptAlreadyExistsException e)
            {
                Logs.ScriptAlreadyExistsCannotBeAdded.FormatWith(path, e.ExistingScript.InvariantName).Log();
                using Page overwrite = new()
                {
                    WindowTitle = AppInfo.Name,
                    Icon = DialogIcon.Warning,
                    Content = ConfirmScriptOverwriteContent.FormatWith(e.ExistingScript.Name),
                    Buttons = { Button.Yes, Button.No },
                };
                retry = Button.Yes.Equals(new Dialog(overwrite).Show());
                if (retry)
                {
                    _ = Scripts.Remove((CheckableScript)e.ExistingScript);
                }
            }
            catch (FileSystemException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page fsErrorPage = DialogPages.FSError(e, new() { Button.Retry, Button.Ignore });
                fsErrorPage.MainInstruction = FSErrorAddingCustomScriptMainInstruction;
                retry = Button.Retry.Equals(new Dialog(fsErrorPage).Show());
            }
        }
    }

    private IEnumerable<string> AskForCustomScriptsToAdd()
    {
        OpenFileDialog ofd = new()
        {
            DefaultExt = App.Settings.ScriptFileExtension,
            Multiselect = true,
            ReadOnlyChecked = true,
        };
        MakeFilter(new(App.Settings.ScriptFileExtension));

        return !ofd.ShowDialog(this) ?? true ? Enumerable.Empty<string>() : ofd.FileNames;

        void MakeFilter(ExtensionGroup group)
        {
            string extensions = string.Join(";", group.Select(ext => $"*{ext}"));
            ofd.Filter = $"{group.Name} ({extensions})|{extensions}";
        }
    }

    private void CheckScripts(Predicate<CheckableScript> check)
    {
        foreach (var script in Scripts)
        {
            script.IsChecked = check(script);
        }
        ScriptCollectionView.Refresh();
    }

    private void RefreshScriptFilter(Category category)
    {
        // Set the filter to keep only the scripts with the current category.
        ScriptCollectionView.Filter = s => ((Script)s).Category == category;

        // DataGridScripts could be null here because this method may be called right when the window is
        // shown, before all elements have initialized.
        if (DataGridScripts is not null && _selectedScriptsDictionary.TryGetValue(category, out var selected))
        {
            // Restore the selected item.
            SelectedScript = selected;
        }
    }

    private void RemoveSelectedScript()
    {
        if (Button.Yes.Equals(new Dialog(DialogPages.DeleteScript).Show()))
        {
            Logs.ScriptRemoved.FormatWith(SelectedScript.AssertNotNull().InvariantName).Log();
            _ = Scripts.Remove(SelectedScript);
            ScriptCollectionView.Refresh();
        }
    }
}