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

            Dialog updateDialog = new(Button.OK)
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
                Current.Shutdown();
            };
        },
        (e, path) =>
        {
            string filename = Path.GetFileName(path);
            Logger.Log(Logs.InvalidScriptData.FormatWith(filename), LogLevel.Error);

            using Dialog deleteScript = new(Button.Yes, Button.No)
            {
                AllowDialogCancellation = true,
                MainIcon = TaskDialogIcon.Warning,
                Content = ConfirmScriptDeletionContent,
                DefaultButton = Button.No
            };

            using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(e, path, Button.DeleteScript, Button.Retry, Button.Ignore);
            invalidScriptData.DefaultButton = Button.Retry;
            invalidScriptData.SetConfirmation(Button.DeleteScript, () => deleteScript.ShowDialog().ClickedButton == Button.Yes);

            Button? result = invalidScriptData.ShowDialog().ClickedButton;
            if (result == Button.DeleteScript)
            {
                File.Delete(path);
                Logger.Log(Logs.ScriptDeleted.FormatWith(filename));
            }

            return result == Button.Retry;
        },
        e =>
        {
            Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical);

            using Dialog dlg = new(Button.Exit)
            {
                MainIcon = TaskDialogIcon.Error,
                Content = UnhandledExceptionDialogContent.FormatWith(e.Message),
                ExpandedInformation = e.ToString(),
                AreHyperlinksEnabled = true
            };
            dlg.HyperlinkClicked += (_, args) =>
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
            dlg.ShowDialog();
        });
}