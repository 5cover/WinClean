using System.Windows;
using System.Windows.Controls;

using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;

using static Scover.WinClean.Resources.UI.Dialogs;

using Page = Scover.Dialogs.Page;

namespace Scover.WinClean.Presentation.Windows;

public partial class MainWindow
{
    private void ButtonAddScriptsClick(object sender, RoutedEventArgs e)
    {
        var oldScriptCount = Scripts.Count;

        foreach (string path in AskForCustomScriptsToAdd())
        {
            AddScript(path);
        }

        if (Scripts.Count > oldScriptCount)
        {
            SelectedScript = Scripts.LastOrDefault();
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

        using Page noScriptsSelected = new()
        {
            IsCancelable = true,
            WindowTitle = AppMetadata.Name,
            Icon = DialogIcon.Error,
            MainInstruction = NoScriptsSelectedMainInstruction,
            Content = NoScriptsSelectedContent,
        };
        _ = new Dialog(noScriptsSelected).Show();
    }

    private void DataGridScriptsDeleteExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) => RemoveSelectedScript();

    private void MenuAboutClick(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void MenuAllClick(object sender, RoutedEventArgs e) => CheckScripts(_ => true);

    private void MenuClearLogsClick(object sender, RoutedEventArgs e) => Task.Run(() => App.Logger.ClearLogs());

    private void MenuExitClick(object sender, RoutedEventArgs e) => Close();

    private void MenuNoneClick(object sender, RoutedEventArgs e) => CheckScripts(_ => false);

    private async void MenuOnlineWikiClick(object sender, RoutedEventArgs e) => (await SourceControlClient.Instance.GetWikiUrl()).Open();

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