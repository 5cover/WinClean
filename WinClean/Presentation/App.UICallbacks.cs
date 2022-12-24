using System.Windows;
using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;
using static Scover.WinClean.Resources.UI.Dialogs;

namespace Scover.WinClean.Presentation;

public partial class App
{
    private static readonly Callbacks uiCallbacks = new(
        () =>
        {
            if (!Settings.ShowUpdateDialog)
            {
                return;
            }
            Page updateDialog = new()
            {
                AllowHyperlinks = true,
                Buttons = new() { Button.OK },
                IsCancelable = true,
                IsMinimizable = true,
                MainInstruction = UpdateMainInstruction,
                Content = UpdateContent.FormatWith(SourceControlClient.Instance.LatestVersionName),
                Icon = DialogIcon.Information,
                Verification = new(UpdateVerificationText)
            };
            updateDialog.HyperlinkClicked += (_, _) => SourceControlClient.Instance.LatestVersionUrl.Open();
            updateDialog.Verification.Checked += (_, _) => Settings.ShowUpdateDialog = !updateDialog.Verification.IsChecked;

            if ((new Dialog(updateDialog).Show(default) as Button)?.Text == Buttons.Cancel)
            {
                // The user probably expects the application to close.
                Current.Shutdown();
            }
        },
        (e, path) =>
        {
            Logger.Log(Logs.ScriptLoadError.FormatWith(path, e), LogLevel.Error);

            var deleteScriptPage = DialogPageFactory.MakeDeleteScript();
            Button deleteScriptButton = new(Buttons.DeleteScript);
            deleteScriptButton.Clicked += (s, e) => e.Cancel = Button.Yes.Equals(new Dialog(deleteScriptPage).Show());

            var invalidScriptDataPage = DialogPageFactory.MakeInvalidScriptData(e, path, new(defaultItem: Button.Retry){ deleteScriptButton, Button.Retry, Button.Ignore });

            Dialog invalidScriptData = new(invalidScriptDataPage);
            var clicked = invalidScriptData.Show();

            if (Buttons.DeleteScript.Equals(clicked))
            {
                File.Delete(path);
                Logger.Log(Logs.ScriptRemoved.FormatWith(path));
            }

            return Button.Retry.Equals(clicked);
        },
        (e, verb, info) =>
        {
            var fsError = DialogPageFactory.MakeFSError(e, verb, info, new(){ Button.Retry, Button.Ignore });
            fsError.MainInstruction = FSErrorLoadingCustomScriptMainInstruction;
            return Button.Retry.Equals(new Dialog(fsError).Show());
        },
        ex =>
        {
            Logger.Log(Logs.UnhandledException.FormatWith(ex), LogLevel.Critical);

            Page unhandledException = new()
            {
                AllowHyperlinks = true,
                Buttons = new() { Buttons.Exit },
                Icon = DialogIcon.Error,
                Content = UnhandledExceptionDialogContent.FormatWith(ex.Message),
                Expander = new(ex.ToString()),
            };
            unhandledException.HyperlinkClicked += (s, e) =>
            {
                switch (e.Href)
                {
                    case "CopyDetails":
                        Clipboard.SetText(ex.ToString());
                        break;

                    case "ReportIssue":
                        SourceControlClient.Instance.NewIssueUrl.Open();
                        break;
                }
            };

            _ = new Dialog(unhandledException).Show();
        });
}