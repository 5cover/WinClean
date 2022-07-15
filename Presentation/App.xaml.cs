global using System.IO;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

using System.Windows;

namespace Scover.WinClean.Presentation;

/// <summary>
/// This class statically stores data only available to the <see cref="Presentation"/> layer. It also handles the startup /
/// shutdown strategy.
/// </summary>
public partial class App
{
    public static ScriptCollection Scripts { get; } = ScriptCollection.LoadScripts((e, path) =>
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
    });

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

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Scripts.Save();

        Happenings.Exit.SetAsHappening();
        Logs.Exiting.Log();
    }

    private void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
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
}