using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.Input;

using Humanizer;

using Microsoft.Win32;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

using WinCopies.Linq;

using Xceed.Wpf.AvalonDock.Controls;

using Button = Scover.WinClean.Presentation.Dialogs.Button;

namespace Scover.WinClean.Presentation.Windows;

public sealed partial class MainWindow
{
    private readonly Dictionary<object, object> _selectedScripts = new();

    public MainWindow()
    {
        InitializeComponent();
        Top = AppInfo.Settings.Top;
        Left = AppInfo.Settings.Left;
        Width = AppInfo.Settings.Width;
        Height = AppInfo.Settings.Height;
        WindowState = AppInfo.Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    public static ObservableCollection<Script> Scripts { get; private set; } = new(App.ScriptCollections.Values.SelectMany(x => x));

    public static RelayCommand<ScriptMetadata> SelectScriptsByMetadata { get; } = new(metadata =>
        {
            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));
            CheckScripts(s =>
            {
                ScriptMetadata? scriptMetadata = metadata switch
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

    private ICollectionView ScriptCollectionView => ((CollectionViewSource)Resources["Scripts"]).View;

    private static void CheckScripts(Predicate<Script> check)
    {
        foreach (var script in Scripts)
        {
            script.IsSelected = check(script);
        }
    }

    private void ButtonAddScriptsClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new()
        {
            DefaultExt = AppInfo.Settings.ScriptFileExtension,
            Multiselect = true,
            ReadOnlyChecked = true,
        };
        MakeFilter(new(AppInfo.Settings.ScriptFileExtension));

        if (!ofd.ShowDialog(this) ?? true)
        {
            return;
        }

        var customScripts = (IMutableScriptCollection)App.ScriptCollections[ScriptType.Custom];
        foreach (string path in ofd.FileNames)
        {
        retry:
            try
            {
                Scripts.Add(customScripts.Add(path));
            }
            catch (InvalidDataException ex)
            {
                Logs.InvalidScriptData.FormatWith(Path.GetFileName(path)).Log(LogLevel.Error);

                using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(ex, path, Button.Retry, Button.Ignore);
                if (invalidScriptData.ShowDialog().ClickedButton == Button.Retry)
                {
                    goto retry;
                }
            }
            catch (ScriptAlreadyExistsException ex)
            {
                using Dialog overwrite = new(Button.Yes, Button.No)
                {
                    MainIcon = TaskDialogIcon.Warning,
                    Content = WinClean.Resources.UI.Dialogs.ConfirmScriptOverwriteContent.FormatWith(ex.ExistingScript.Name)
                };
                overwrite.DefaultButton = Button.Yes;
                if (overwrite.ShowDialog().ClickedButton == Button.Yes)
                {
                    customScripts.Remove(ex.ExistingScript);
                    _ = Scripts.Remove(ex.ExistingScript);
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

        void MakeFilter(ExtensionGroup group)
        {
            string extensions = string.Join(";", group.Select(ext => $"*{ext}"));
            ofd.Filter = $"{group.Name} ({extensions})|{extensions}";
        }
    }

    private void ButtonExecuteScriptsClick(object sender, RoutedEventArgs e)
    {
        var selectedScripts = Scripts.Where(s => s.IsSelected);
        if (!selectedScripts.Any())
        {
            using Dialog noScriptsSelected = new(Button.OK)
            {
                MainIcon = TaskDialogIcon.Error,
                MainInstruction = WinClean.Resources.UI.Dialogs.NoScriptsSelectedMainInstruction,
                Content = WinClean.Resources.UI.Dialogs.NoScriptsSelectedContent
            };
            _ = noScriptsSelected.ShowDialog();
            return;
        }
        using ScriptExecutionWizard wizard = new(selectedScripts);
        wizard.Execute();
    }

    private void MenuAboutClick(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void MenuAllClick(object sender, RoutedEventArgs e) => CheckScripts(_ => true);

    private void MenuClearLogsClick(object sender, RoutedEventArgs e) => Task.Run(() => App.Logger.ClearLogs());

    private void MenuExitClick(object sender, RoutedEventArgs e) => Close();

    private void MenuNoneClick(object sender, RoutedEventArgs e) => CheckScripts(_ => false);

    private void MenuOnlineWikiClick(object sender, RoutedEventArgs e) => Helpers.Open(AppInfo.Settings.WikiUrl);

    private void MenuOpenLogsDirClick(object sender, RoutedEventArgs e) => Helpers.OpenExplorerToDirectory(AppDirectory.Logs);

    private void MenuOpenScriptsDirClick(object sender, RoutedEventArgs e) => Helpers.Open(AppDirectory.Scripts);

    private void MenuSettingsClick(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();

    private void ScriptEditorScriptChangedCategory(object sender, EventArgs e) => ScriptCollectionView.Refresh();

    private void ScriptEditorScriptRemoved(object sender, EventArgs e) => ScriptCollectionView.Refresh();

    private void TabControlCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var category = TabControlCategories.SelectedItem;

        if (e.OriginalSource is DataGrid dgScripts && dgScripts.SelectedItem is not null)
        {
            _selectedScripts[category] = dgScripts.SelectedItem;
            return;
        }

        DataGrid? currentDgScripts = TabControlCategories.FindVisualChildren<DataGrid>().SingleOrDefault();

        // 1. Commit any pending edits and exit edit mode before setting the filter.
        _ = currentDgScripts?.CommitEdit(DataGridEditingUnit.Row, true);

        // 2. Set the filter to keep only the scripts with the current category.
        ScriptCollectionView.Filter = s => ((Script)s).Category.Equals(category);
        // 3. Refresh to apply changes
        ScriptCollectionView.Refresh();

        // 4. Refreshing unfocuses the DataGrid, so focus it.
        _ = currentDgScripts?.Focus();

        if (currentDgScripts is not null && _selectedScripts.TryGetValue(category, out var selected))
        {
            // 5. Restore the selected item, if available.
            currentDgScripts.SelectedItem = selected;
        }
    }

    private void WindowClosed(object sender, EventArgs e)
    {
        AppInfo.Settings.Top = Top;
        AppInfo.Settings.Left = Left;
        AppInfo.Settings.Width = Width;
        AppInfo.Settings.Height = Height;
        AppInfo.Settings.IsMaximized = WindowState == WindowState.Maximized;
    }
}