using Microsoft.Win32;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.Logic;
using Scover.WinClean.Operational;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.ScriptExecution;

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace Scover.WinClean.Presentation.Windows;

public partial class MainWindow : Window
{
    private IEnumerable<Script> _scripts;

    public MainWindow()
    {
        InitializeComponent();
        ReloadScripts();
        ResetTabs();
    }

    private IEnumerable<Script> Scripts => tabControlCategories.Items.OfType<TabItem>().SelectMany(tabItem => ((ListBox)tabItem.Content).Items.Cast<Script>());

    private void ButtonAddScripts_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new()
        {
            DefaultExt = ".xml",
            Multiselect = true,
            ReadOnlyChecked = true
        };
        ofd.MakeFilter(new ExtensionGroup(new string[1] { ".xml" }));

        if (ofd.ShowDialog(this) ?? false)
        {
            foreach (string filePath in ofd.FileNames)
            {
                string destPath = ScriptsDir.Instance.Join(Path.GetFileName(filePath));
                if (!File.Exists(destPath))
                {
                    File.Copy(filePath, destPath);
                }
            }

            ReloadScripts();
            ResetTabs();
        }
    }

    private void ButtonExecuteScripts_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            new ScriptExecutionWizard(Scripts.Where(s => s.Selected)).ExecuteUI();
        }
        catch (InvalidOperationException)
        {
            using CustomDialog noScriptsSelected = new(WinClean.Resources.UI.Buttons.Close)
            {
                MainIcon = TaskDialogIcon.Error,
                MainInstruction = WinClean.Resources.UI.MainWindow.NoScriptsSelectedMainInstruction,
                Content = WinClean.Resources.UI.MainWindow.NoScriptsSelectedContent
            };
            _ = noScriptsSelected.ShowDialog();
        }
    }

    private void CheckScripts(bool check) => CheckScripts(_ => check);

    private void CheckScripts(Predicate<Script> check)
    {
        IEnumerable<ListBox>? listBoxes = tabControlCategories.Items.OfType<TabItem>().Select(item => (ListBox)item.Content);

        foreach (Script script in Scripts)
        {
            script.Selected = check(script);
        }
    }

    private void MainWindow_Closed(object sender, EventArgs e)
    {
        ScriptXmlSerializer serializer = new(ScriptsDir.Instance.Info);
        foreach (Script script in Scripts)
        {
            serializer.Serialize(script);
        }
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e) => new AboutWindow() { Owner = this }.ShowDialog();

    private void MenuAdvised_Click(object sender, RoutedEventArgs e) => CheckScripts(script => script.Advised == Advised.Yes);

    private void MenuAll_Click(object sender, RoutedEventArgs e) => CheckScripts(true);

    private void MenuClearLogs_Click(object sender, RoutedEventArgs e) => Logger.Instance.ClearLogsFolderAsync();

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    private void MenuNone_Click(object sender, RoutedEventArgs e) => CheckScripts(false);

    private void MenuOnlineWiki_Click(object sender, RoutedEventArgs e) => Helpers.OpenUrl(new(App.Settings.WikiUrl));

    private void MenuSettings_Click(object sender, RoutedEventArgs e) => new SettingsWindow() { Owner = this }.ShowDialog();

    [MemberNotNull(nameof(_scripts))]
    private void ReloadScripts() => _scripts = ScriptsDir.Instance.LoadAllScripts();

    /// <summary>Recreates the items of <see cref="tabControlCategories"/> and redistributes the scripts.</summary>
    private void ResetTabs()
    {
        int selectedIndex = tabControlCategories.SelectedIndex;
        tabControlCategories.Items.Clear();

        foreach (Category category in Category.Values)
        {
            ListBox ListBox = new()
            {
                ItemsSource = _scripts.Where(s => s.Category == category),
            };
            TabItem tabItem = new()
            {
                Header = category.LocalizedName,
                Content = ListBox,
            };
            _ = tabControlCategories.Items.Add(tabItem);
        }

        tabControlCategories.SelectedIndex = selectedIndex;
    }

    private void ScriptEditor_ScriptChangedCategory(object sender, ScriptChangedCategoryEventArgs e) => ResetTabs();

    private void TabControlCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 1 && e.RemovedItems[0] is TabItem item)
        {
            ((ListBox)item.Content).SelectedItem = null;
        }
    }
}