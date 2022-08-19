global using System.IO;
using System.Windows;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

/// <summary>
/// This class statically stores data only available to the <see cref="Presentation"/> layer. It also handles the startup /
/// shutdown strategy.
/// </summary>
public partial class App
{
    private static readonly InvalidScriptDataCallback _invalidScriptDataCallback = (e, path) =>
    {
        string filename = Path.GetFileName(path);
        Logs.InvalidScriptData.FormatWith(filename).Log(LogLevel.Error);

        using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(e, path, Button.DeleteScript, Button.Retry, Button.Ignore);
        invalidScriptData.DefaultButton = Button.Retry;
        using Dialog deleteScript = DialogFactory.MakeScriptDeletionConfirmation();

        invalidScriptData.SetConfirmation(Button.DeleteScript, () => deleteScript.ShowDialog() == Button.Yes);

        Button? result = invalidScriptData.ShowDialog();
        if (result == Button.DeleteScript)
        {
            File.Delete(path);
            Logs.ScriptDeleted.FormatWith(filename).Log();
        }

        return result == Button.Retry;
    };

    public static ScriptCollection Scripts { get; } = ScriptCollection.LoadScripts(AppDirectory.ScriptsDir, _invalidScriptDataCallback);

    private static void ShowUnhandledExceptionDialog(Exception e)
    {
        Happenings.Exception.SetAsHappening();
        Logs.UnhandledException.FormatWith(e).Log(LogLevel.Critical);
        using Dialog unhandledExceptionDialog = new(Button.Exit, Button.CopyDetails)
        {
            MainIcon = TaskDialogIcon.Error,
            Content = WinClean.Resources.UI.Dialogs.UnhandledExceptionDialogContent.FormatWith(e.Message),
            ExpandedInformation = e.ToString(),
            EnableHyperlinks = true
        };
        unhandledExceptionDialog.SetConfirmation(Button.CopyDetails, () =>
        {
            Clipboard.SetText(e.ToString());
            return false;
        });
        unhandledExceptionDialog.HyperlinkClicked += (_, args) => Helpers.Open(args.Href);
        unhandledExceptionDialog.ShowDialog();
    }

    private static void StartConsole(string[] args)
    {
        WarnIfNewVersionAvailable(() => Console.WriteLine(WinClean.Resources.UI.Dialogs
                          .NewVersionAvailableContent.FormatWith(SourceControlClient.Instance.Value.LatestVersionName)));
        //Environment.ExitCode = new CommandLineInterpreter(args).Execute();
    }

    private static void StartGui()
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

        AppInfo.ReadAppFileRetryOrFail = (ex, verb, info) =>
        {
            using FSErrorDialog dialog = new(ex, verb, info, Button.Retry, Button.Exit);
            return dialog.ShowDialog() == Button.Retry;
        };

        Current.DispatcherUnhandledException += (_, args) => ShowUnhandledExceptionDialog(args.Exception);
        Happenings.Start.SetAsHappening();
        Logs.Started.Log();
        new MainWindow().Show();
    }

    private static void StartConsole(IEnumerable<string> args)
    {
        //Environment.ExitCode = new CommandLineInterpreter(args).Execute();
    }
}