using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;

using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Button = Scover.WinClean.Presentation.Dialogs.Button;

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

        var oldScriptCount = Scripts.Count;

        foreach (string path in ofd.FileNames)
        {
            AddScript(path);
        }

        // If new scripts were added, select the last added script.
        if (Scripts.Count > oldScriptCount)
        {
            SelectedScript = Scripts.LastOrDefault();
        }

        void MakeFilter(ExtensionGroup group)
        {
            string extensions = string.Join(";", group.Select(ext => $"*{ext}"));
            ofd.Filter = $"{group.Name} ({extensions})|{extensions}";
        }
    }

    private void ButtonExecuteScriptsClick(object sender, RoutedEventArgs e)
    {
        var selectedScripts = Scripts.Where(s => s.IsSelected).ToList();
        if (!selectedScripts.Any())
        {
            using Dialog noScriptsSelected = new(Button.Ok)
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

    private void MenuOpenLogsDirClick(object sender, RoutedEventArgs e) => Helpers.Open(AppDirectory.Logs);

    private void MenuOpenScriptsDirClick(object sender, RoutedEventArgs e) => Helpers.Open(AppDirectory.Scripts);

    private void MenuSettingsClick(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();

    private void ScriptEditorScriptChangedCategory(object sender, EventArgs e) => ScriptCollectionView.Refresh();

    private void ScriptEditorScriptRemoved(object sender, EventArgs e)
    {
        _ = Scripts.Remove(SelectedScript.AssertNotNull());
        ScriptCollectionView.Refresh();
    }

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
        AppInfo.Settings.Top = Top;
        AppInfo.Settings.Left = Left;
        AppInfo.Settings.Width = Width;
        AppInfo.Settings.Height = Height;
        AppInfo.Settings.IsMaximized = WindowState == WindowState.Maximized;
        App.SaveScripts(Scripts);
    }
}