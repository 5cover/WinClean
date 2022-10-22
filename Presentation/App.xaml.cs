global using System.IO;

global using static Humanizer.StringExtensions;

using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;

using CommandLine;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

using static Scover.WinClean.Resources.UI.Dialogs;

namespace Scover.WinClean.Presentation;

/// <summary>Handles the startup / shutdown strategy and hold data related to the Presentation layer.</summary>
public partial class App
{
    private static Logger? _logger;
    private static ScriptCollection? _scripts;

    /// <summary>The application's logger, available after startup.</summary>
    public static Logger Logger { get => _logger.AssertNotNull(); }

    /// <summary>The application's scripts, available after startup.</summary>
    public static ScriptCollection Scripts { get => _scripts.AssertNotNull(); }

    private static void Initialize(Logger logger,
                              Action warnUserOnUpdate,
                              InvalidScriptDataCallback reloadElseIgnore,
                              Action<Exception> onUnhandledException,
                              FSOperationCallback retryElseFail)
    {
        //1. Set the logger
        _logger = logger;

        //2. Add the unhandled exception handler
        Current.DispatcherUnhandledException += (_, args) => onUnhandledException(args.Exception);

        //3. Set the app file callback
        AppInfo.OpenAppFileRetryElseFail = retryElseFail;

        //4. Check for updates
        if (SourceControlClient.Instance.Value.LatestVersionName != AppInfo.Version)
        {
            warnUserOnUpdate();
        }

        //5. Load scripts.
        _scripts = ScriptCollection.LoadScripts(AppDirectory.ScriptsDir, reloadElseIgnore);
    }

    private static void StartConsole(string[] args)
    {
        // Get invariant (en-US) output.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        // Enable console
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);
        const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;
        AttachConsole(ATTACH_PARENT_PROCESS);

        Initialize(new ConsoleLogger(),
            () => Logger.Log(WinClean.Resources.CommandLine.Update.FormatWith(SourceControlClient.Instance.Value.LatestVersionName,
                                                                              SourceControlClient.Instance.Value.LatestVersionUrl), LogLevel.Info),
            (e, path) =>
            {
                // Log the error, but ignore invalid scripts.
                Logger.Log($"{Logs.InvalidScriptData.FormatWith(Path.GetFileName(path))}\n{e}", LogLevel.Error);
                return false;
            }, e => Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical),
            (ex, verb, info) =>
                // Fail immediately. Let it turn into an unhandled exception.
                false);
        _ = Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(options => Environment.ExitCode = options.Execute());

        Current.Shutdown();
    }

    private static void StartGui()
    {
        Initialize(new CsvLogger(), () =>
        {
            Dialog updateDialog = new(Button.OK)
            {
                MainInstruction = UpdateMainInstruction,
                Content = UpdateContent.FormatWith(SourceControlClient.Instance.Value.LatestVersionName),
                AllowDialogCancellation = true,
                ShowMinimizeBox = true,
                MainIcon = TaskDialogIcon.Information,
                AreHyperlinksEnabled = true
            };
            updateDialog.HyperlinkClicked += (_, e) => Helpers.Open(SourceControlClient.Instance.Value.LatestVersionUrl);

            if (updateDialog.Show().WasClosed)
            {
                Current.Shutdown();
            };
        },
        (e, path) =>
        {
            string filename = Path.GetFileName(path);
            Logger.Log(Logs.InvalidScriptData.FormatWith(filename), LogLevel.Error);

            using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(e, path, Button.DeleteScript, Button.Retry, Button.Ignore);
            invalidScriptData.DefaultButton = Button.Retry;

            using Dialog deleteScript = new(Button.Yes, Button.No)
            {
                AllowDialogCancellation = true,
                MainIcon = TaskDialogIcon.Warning,
                Content = ConfirmScriptDeletionContent,
                DefaultButton = Button.No
            };

            invalidScriptData.SetConfirmation(Button.DeleteScript, () => deleteScript.ShowDialog().ClickedButton == Button.Yes);

            Button? result = invalidScriptData.ShowDialog().ClickedButton;
            if (result == Button.DeleteScript)
            {
                File.Delete(path);
                Logger.Log(Logs.ScriptDeleted.FormatWith(filename));
            }

            return result == Button.Retry;
        }, e =>
        {
            Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical);
            using Dialog unhandledExceptionDialog = new(Button.Exit, Button.CopyDetails)
            {
                MainIcon = TaskDialogIcon.Error,
                Content = UnhandledExceptionDialogContent.FormatWith(e.Message),
                ExpandedInformation = e.ToString(),
                AreHyperlinksEnabled = true
            };
            unhandledExceptionDialog.SetConfirmation(Button.CopyDetails, () =>
            {
                Clipboard.SetText(e.ToString());
                return false;
            });
            unhandledExceptionDialog.HyperlinkClicked += (_, args) => Helpers.Open(args.Href);
            unhandledExceptionDialog.ShowDialog();
        }, (ex, verb, info) =>
        {
            using FSErrorDialog dialog = new(ex, verb, info, Button.Retry, Button.Exit);
            return dialog.ShowDialog().ClickedButton == Button.Retry;
        });
        new MainWindow().Show();
    }

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Scripts.Save();
        AppInfo.Settings.Save();

        Logger.Log(Logs.Exiting);
    }

    private void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        if (e?.Args.Any() ?? false)
        {
            StartConsole(e.Args);
        }
        else
        {
            StartGui();
        }
    }
}