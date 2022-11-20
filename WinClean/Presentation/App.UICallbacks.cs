using System.Windows;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

using static Scover.WinClean.Resources.UI.Dialogs;

namespace Scover.WinClean.Presentation;

public partial class App
{
    private static readonly Callbacks uiCallbacks = new(
        () =>
        {
            if (!AppInfo.Settings.ShowUpdateDialog)
            {
                return;
            }

            using Dialog updateDialog = new(Button.Ok)
            {
                MainInstruction = UpdateMainInstruction,
                Content = UpdateContent.FormatWith(SourceControlClient.Instance.Value.LatestVersionName),
                AllowDialogCancellation = true,
                ShowMinimizeBox = true,
                MainIcon = TaskDialogIcon.Information,
                AreHyperlinksEnabled = true,
                VerificationText = UpdateVerificationText,
            };
            updateDialog.HyperlinkClicked += (_, _) => Helpers.Open(SourceControlClient.Instance.Value.LatestVersionUrl);
            updateDialog.VerificationClicked += (_, _) => AppInfo.Settings.ShowUpdateDialog = !updateDialog.IsVerificationChecked;

            if (updateDialog.Show().WasClosed)
            {
                // The user probably expects the application to close.
                Current.Shutdown();
            }
        },
        (e, path) =>
        {
            Logger.Log(Logs.InvalidScriptData.FormatWith(path, e), LogLevel.Error);

            using Dialog deleteScript = DialogFactory.MakeDeleteScriptDialog();

            using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(e, path, Button.DeleteScript, Button.Retry, Button.Ignore);
            invalidScriptData.DefaultButton = Button.Retry;
            invalidScriptData.SetConfirmation(Button.DeleteScript, () => deleteScript.ShowDialog().ClickedButton == Button.Yes);

            Button? result = invalidScriptData.ShowDialog().ClickedButton;
            if (result == Button.DeleteScript)
            {
                File.Delete(path);
                Logger.Log(Logs.ScriptDeleted.FormatWith(path));
            }

            return result == Button.Retry;
        },
        (e, verb, info) =>
        {
            using FSErrorDialog fsErrorDialog = new(e, verb, info, Button.Retry, Button.Ignore)
            {
                MainInstruction = FSErrorLoadingCustomScriptMainInstruction
            };
            return fsErrorDialog.ShowDialog().ClickedButton == Button.Retry;
        },
        e =>
        {
            Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical);

            using Dialog unhandledException = new(Button.Exit)
            {
                MainIcon = TaskDialogIcon.Error,
                Content = UnhandledExceptionDialogContent.FormatWith(e.Message),
                ExpandedInformation = e.ToString(),
                AreHyperlinksEnabled = true
            };
            unhandledException.HyperlinkClicked += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Href))
                {
                    Clipboard.SetText(e.ToString());
                }
                else
                {
                    Helpers.Open(args.Href);
                }
            };
            _ = unhandledException.ShowDialog();
        });
}