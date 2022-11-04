using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.Input;

using Humanizer;

using Microsoft.Win32;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Presentation.ScriptExecution;
using Scover.WinClean.Resources;

using Button = Scover.WinClean.Presentation.Dialogs.Button;

namespace Scover.WinClean.Presentation.Windows;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        RestoreSizeAndLocation();
        ResetTabs();
    }

    public static RelayCommand<ScriptMetadata> SelectScriptsByMetadata { get; } = new(metadata => CheckScripts(s =>
    {
        ScriptMetadata scriptMetadata = metadata switch
        {
            Category _ => s.Category,
            RecommendationLevel _ => s.RecommendationLevel,
            Host _ => s.Host,
            Impact _ => s.Impact,
            _ => throw new ArgumentException("Has an invalid type or is null", nameof(metadata))
        };
        return scriptMetadata == metadata;
    }));

    public static RelayCommand<char> SelectScriptSelectionOperator { get; } = new(@operator =>
    {
    });

    private static void CheckScripts(Predicate<Script> check)
    {
        foreach (Script script in App.AllScripts)
        {
            script.IsSelected = check(script);
        }
    }

    private static void MakeFilter(FileDialog ofd, params ExtensionGroup[] extensions)
        => ofd.Filter = string.Join('|', extensions.SelectMany(group => new[]
        {
            $"{group.Name} ({string.Join(';', group.Select(ext => $"*{ext}"))})",
            string.Join(';', group.Select(ext => $"*{ext}"))
        }));

    private void ButtonAddScriptsClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new()
        {
            DefaultExt = AppInfo.Settings.ScriptFileExtension,
            Multiselect = true,
            ReadOnlyChecked = true,
            ValidateNames = false
        };
        MakeFilter(ofd, new ExtensionGroup(AppInfo.Settings.ScriptFileExtension));

        if (!(ofd.ShowDialog(this) ?? false))
        {
            return;
        }

        foreach (string filePath in ofd.FileNames)
        {
            AddScript(filePath);
        }

        ResetTabs();

        static void AddScript(string filePath)
        {
            bool allowOverwrite = false;
            while (true)
            {
                try
                {
                    App.CustomScripts.AddNew(filePath, allowOverwrite);
                    break;
                }
                catch (InvalidDataException ex)
                {
                    Logs.InvalidScriptData.FormatWith(Path.GetFileName(filePath)).Log(LogLevel.Error);

                    using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(ex, filePath, Button.Retry, Button.Ignore);
                    if (invalidScriptData.ShowDialog().ClickedButton != Button.Retry)
                    {
                        break;
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
                    allowOverwrite = overwrite.ShowDialog().ClickedButton == Button.Yes;
                }
                catch (Exception ex) when (ex.IsFileSystem())
                {
                    using FSErrorDialog fsErrorDialog = new(ex, FSVerb.Access, new FileInfo(filePath), Button.Retry, Button.Ignore)
                    {
                        MainInstruction = WinClean.Resources.UI.Dialogs.FSErrorAddingScriptMainInstruction
                    };
                    DialogResult result = fsErrorDialog.ShowDialog();
                    if (result.WasClosed || result.ClickedButton == Button.Ignore)
                    {
                        break;
                    }
                }
            }
        }
    }

    private void ButtonExecuteScriptsClick(object sender, RoutedEventArgs e)
    {
        var selectedScripts = App.AllScripts.Where(s => s.IsSelected);
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
        IsEnabled = false;
        wizard.Execute();
        IsEnabled = true;
    }

    private void MenuAboutClick(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void MenuAllClick(object sender, RoutedEventArgs e) => CheckScripts(_ => true);

    private void MenuClearLogsClick(object sender, RoutedEventArgs e) => (App.Logger as CsvLogger)?.ClearLogsFolderAsync();

    private void MenuExitClick(object sender, RoutedEventArgs e) => Close();

    private void MenuNoneClick(object sender, RoutedEventArgs e) => CheckScripts(_ => false);

    private void MenuOnlineWikiClick(object sender, RoutedEventArgs e) => Helpers.Open(AppInfo.Settings.WikiUrl);

    private void MenuOpenLogsDirClick(object sender, RoutedEventArgs e) => Helpers.OpenExplorerToDirectory(AppDirectory.LogDir.Info.FullName);

    private void MenuOpenScriptsDirClick(object sender, RoutedEventArgs e) => Helpers.Open(AppDirectory.ScriptsDir.Info.FullName);

    private void MenuSettingsClick(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();

    /// <summary>Recreates the items of <see cref="TabControlCategories"/> and redistributes the scripts.</summary>
    private void ResetTabs()
    {
        int selectedIndex = TabControlCategories.SelectedIndex;

        TabControlCategories.Items.Clear();

        foreach (Category category in AppInfo.Categories.Value.Values)
        {
            ListBox listBox = new()
            {
                ItemsSource = App.AllScripts.Where(s => s.Category == category),
            };
            TabItem tabItem = new()
            {
                Header = category.Name,
                Content = listBox,
                ToolTip = category.Description
            };
            _ = TabControlCategories.Items.Add(tabItem);
        }

        TabControlCategories.SelectedIndex = selectedIndex;
    }

    private void RestoreSizeAndLocation()
    {
        Top = AppInfo.Settings.Top;
        Left = AppInfo.Settings.Left;
        Width = AppInfo.Settings.Width;
        Height = AppInfo.Settings.Height;
        WindowState = AppInfo.Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    private void SaveSizeAndLocation()
    {
        AppInfo.Settings.Top = Top;
        AppInfo.Settings.Left = Left;
        AppInfo.Settings.Width = Width;
        AppInfo.Settings.Height = Height;
        AppInfo.Settings.IsMaximized = WindowState == WindowState.Maximized;
    }

    private void ScriptEditorScriptChangedCategory(object sender, EventArgs e) => ResetTabs();

    private void ScriptEditorScriptRemoved(object sender, EventArgs e) => ResetTabs();

    private void TabControlCategoriesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 1 && e.RemovedItems[0] is TabItem item)
        {
            ((ListBox)item.Content).SelectedItem = null;
        }
    }

    private void Window_Closed(object sender, EventArgs e) => SaveSizeAndLocation();
}