using System.Windows;
using System.Windows.Controls;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.Dialogs;
using static Scover.WinClean.Resources.UI.Dialogs;
using Page = Scover.Dialogs.Page;
using Button = Scover.Dialogs.Button;

namespace Scover.WinClean.Presentation.Windows;

public partial class MainWindow
{
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

        foreach (string path in ofd.FileNames)
        {
            AddScript(path);
        }

        SelectedScript = Scripts.LastOrDefault();

        void MakeFilter(ExtensionGroup group)
        {
            string extensions = string.Join(";", group.Select(ext => $"*{ext}"));
            ofd.Filter = $"{group.Name} ({extensions})|{extensions}";
        }
    }

    private void ButtonExecuteScriptsClick(object sender, RoutedEventArgs e)
    {
        var selectedScripts = Scripts.Where(s => s.IsSelected).ToList();

        if (selectedScripts.Any())
        {
            new ScriptExecutionWizard(selectedScripts).Execute();
            return;
        }

        Page noScriptsSelected = new()
        {
            Icon = DialogIcon.Error,
            MainInstruction = NoScriptsSelectedMainInstruction,
            Content = NoScriptsSelectedContent
        };
        _ = new Dialog(noScriptsSelected).Show();
    }

    private void DataGridScriptsDeleteExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => RemoveSelectedScript();

    private void MenuAboutClick(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void MenuAllClick(object sender, RoutedEventArgs e) => CheckScripts(_ => true);

    private void MenuClearLogsClick(object sender, RoutedEventArgs e) => Task.Run(() => App.Logger.ClearLogs());

    private void MenuExitClick(object sender, RoutedEventArgs e) => Close();

    private void MenuNoneClick(object sender, RoutedEventArgs e) => CheckScripts(_ => false);

    private void MenuOnlineWikiClick(object sender, RoutedEventArgs e) => App.Settings.WikiUrl.Open();

    private void MenuOpenLogsDirClick(object sender, RoutedEventArgs e) => AppDirectory.Logs.Open();

    private void MenuOpenScriptsDirClick(object sender, RoutedEventArgs e) => AppDirectory.Scripts.Open();

    private void MenuSettingsClick(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();

    private void ScriptEditorScriptChangedCategory(object sender, EventArgs e) => ScriptCollectionView.Refresh();

    private void ScriptEditorScriptRemoved(object sender, EventArgs e) => RemoveSelectedScript();

    private void TabControlCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var category = (Category)TabControlCategories.SelectedItem;

        if (e.OriginalSource is DataGrid dgScripts)
        {
            if (dgScripts.SelectedItem is not null)
            {
                _selectedScripts[category] = (Script)dgScripts.SelectedItem;
            }
            return;
        }
        RefreshScriptFilter(category);
    }

    private void WindowClosed(object sender, EventArgs e)
    {
        App.Settings.Top = Top;
        App.Settings.Left = Left;
        App.Settings.Width = Width;
        App.Settings.Height = Height;
        App.Settings.IsMaximized = WindowState == WindowState.Maximized;
        App.SaveScripts(Scripts);
    }
}