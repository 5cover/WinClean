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

        Happenings.Start.SetAsHappening();
        Logs.Started.Log();

        try
        {
            new MainWindow().Show();
        }
        catch (Exception ex)
        {
            ShowUnhandledExceptionDialog(ex);
            // throw will be a dirty exit, so we need to call the ApplicationExit event handler manually.
            ApplicationExit(null, null);
            throw;
        }
    }

    private static void ShowUnhandledExceptionDialog(Exception e)
    {
        Happenings.Exception.SetAsHappening();
        Logs.UnhandledException.FormatWith(e).Log(LogLevel.Critical);
        using Dialog unhandledExceptionDialog = new(Button.Exit)
        {
            MainIcon = TaskDialogIcon.Error,
            Content = WinClean.Resources.UI.Dialogs.UnhandledExceptionDialogContent,
            ExpandedInformation = e.ToString()
        };
        unhandledExceptionDialog.Show();
    }
}