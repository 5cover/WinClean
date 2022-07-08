using Microsoft.Win32;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.ScriptExecution;
using Scover.WinClean.Resources;

using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Scover.WinClean.Presentation.Windows;

public partial class MainWindow : Window
{
    private IEnumerable<Script> _scripts;

    public MainWindow()
    {
        InitializeComponent();
        _scripts = LoadAllScripts();
        ResetTabs();
    }

    private IEnumerable<Script> Scripts => tabControlCategories.Items.OfType<TabItem>().SelectMany(tabItem => ((ListBox)tabItem.Content).Items.Cast<Script>());

    /// <summary>Loads all the scripts present in the scripts directory.</summary>
    /// <remarks>Will not load scripts located in subdirectories.</remarks>
    private static IEnumerable<Script> LoadAllScripts()
    {
        List<Script> scripts = new();

        Happenings.ScriptLoading.SetAsHappening();

        IScriptSerializer serializer = new ScriptXmlSerializer();

        foreach (FileInfo script in AppDirectory.ScriptsDir.Info.EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly))
        {
            Script? deserializedScript = null;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    deserializedScript = serializer.Deserialize(script);
                }
                catch (Exception e) when (e is System.Xml.XmlException or ArgumentException)
                {
                    Logs.BadScriptData.FormatWith(script.Name).Log(LogLevel.Error);

                    using CustomDialog badScriptData = new(ScriptsDirectory.EditTheScriptAndRetry, ScriptsDirectory.DeleteTheScript)
                    {
                        MainIcon = TaskDialogIcon.Error,
                        Content = ScriptsDirectory.BadScriptDataDialogContent.FormatWith(script.Name),
                        ExpandedInformation = e.ToString()
                    };
                    string result = badScriptData.ShowDialog();

                    if (result == ScriptsDirectory.EditTheScriptAndRetry)
                    {
                        using Process notepad = Process.Start("notepad", script.FullName);
                        Logs.NotepadOpened.Log();

                        notepad.WaitForExit();

                        retry = true;
                    }
                    else if (result == ScriptsDirectory.DeleteTheScript && YesNoDialog.ScriptDeletion.ShowDialog() == Dialogs.DialogResult.Yes)
                    {
                        script.Delete();
                        Logs.ScriptDeleted.FormatWith(script.Name).Log(LogLevel.Info);
                    }
                }
                catch (Exception e) when (e.FileSystem())
                {
                    Logs.FileSystemErrorAcessingScript.FormatWith(script.Name).Log(LogLevel.Error);
                    retry = FSErrorFactory.MakeFSError<RetryIgnoreExitDialog>(e, FSVerb.Acess, script).ShowDialog() == Dialogs.DialogResult.Retry;
                }
            } while (retry);
            if (deserializedScript is not null)
            {
                scripts.Add(deserializedScript);
            }
        }

        Logs.ScriptsLoaded.FormatWith(scripts.Count).Log(LogLevel.Info);

        return scripts;
    }

    private static void MakeFilter(OpenFileDialog ofd, params ExtensionGroup[] exts)
                        => ofd.Filter = new StringBuilder().AppendJoin('|', exts.SelectMany(group => new string[]
                                                                              {
                                                                                  $"{group.GetName(0)} ({string.Join(';', group.Select(ext => $"*{ext}"))})",
                                                                                  string.Join(';', group.Select(ext => $"*{ext}"))
                                                                              })).ToString();

    private void ButtonAddScripts_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new()
        {
            DefaultExt = ".xml",
            Multiselect = true,
            ReadOnlyChecked = true
        };
        MakeFilter(ofd, new ExtensionGroup(".xml"));

        if (ofd.ShowDialog(this) ?? false)
        {
            foreach (string filePath in ofd.FileNames)
            {
                string destPath = AppDirectory.ScriptsDir.Join(Path.GetFileName(filePath));
                if (!File.Exists(destPath))
                {
                    File.Copy(filePath, destPath);
                }
            }

            _scripts = LoadAllScripts();
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
        ScriptXmlSerializer serializer = new();
        foreach (Script script in Scripts)
        {
            serializer.Serialize(script);
        }
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e) => new AboutWindow() { Owner = this }.ShowDialog();

    private void MenuAll_Click(object sender, RoutedEventArgs e) => CheckScripts(true);

    private void MenuClearLogs_Click(object sender, RoutedEventArgs e) => Logger.Instance.ClearLogsFolderAsync();

    private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

    private void MenuNone_Click(object sender, RoutedEventArgs e) => CheckScripts(false);

    private void MenuOnlineWiki_Click(object sender, RoutedEventArgs e) => Helpers.OpenUrl(new(App.Settings.WikiUrl));

    private void MenuRecommended_Click(object sender, RoutedEventArgs e) => CheckScripts(script => script.Recommended == BusinessLogic.RecommendationLevel.FromName("Yes"));

    private void MenuSettings_Click(object sender, RoutedEventArgs e) => new SettingsWindow() { Owner = this }.ShowDialog();

    /// <summary>Recreates the items of <see cref="tabControlCategories"/> and redistributes the scripts.</summary>
    private void ResetTabs()
    {
        int selectedIndex = tabControlCategories.SelectedIndex;
        tabControlCategories.Items.Clear();

        foreach (BusinessLogic.Category category in BusinessLogic.Category.Values)
        {
            ListBox ListBox = new()
            {
                ItemsSource = _scripts.Where(s => s.Category == category),
            };
            TabItem tabItem = new()
            {
                Header = category.Name,
                Content = ListBox,
                ToolTip = category.Description
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