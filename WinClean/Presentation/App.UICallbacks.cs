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
        NotifyUpdateAvailable: async () =>
        {
            var scc = await SourceControlClient.Instance;
            using Page update = new()
            {
                AllowHyperlinks = true,
                IsCancelable = true,
                MainInstruction = Update.MainInstruction,
                Sizing = Sizing.Content,
                Icon = DialogIcon.Information,
                Content = Update.Content.FormatWith(scc.LatestVersionName),
                Verification  = new(Update.Verification),
            };
            update.HyperlinkClicked += (_, _) => scc.LatestVersionUrl.Open();
            update.Verification.Checked += (_, _) => Settings.ShowUpdateDialog ^= true;
            _ = new Dialog(update).Show();
        },
        InvalidScriptData: (e, path) =>
        {
            Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);

            using Page deleteScriptPage = DialogPages.DeleteScript;
            Button deleteScriptButton = new(Buttons.DeleteScript);
            deleteScriptButton.Clicked += (s, e) => e.Cancel = Button.Yes.Equals(new Dialog(deleteScriptPage).Show());

            using Page invalidScriptDataPage = DialogPages.InvalidScriptData(e, path, new(defaultItem: Button.Retry){ deleteScriptButton, Button.Retry, Button.Ignore });

            Dialog invalidScriptData = new(invalidScriptDataPage);
            var clicked = invalidScriptData.Show();

            if (Buttons.DeleteScript.Equals(clicked))
            {
                Logs.ScriptRemoved.FormatWith(path).Log();
                return InvalidScriptDataAction.Remove;
            }

            return Button.Retry.Equals(clicked) ? InvalidScriptDataAction.Reload : InvalidScriptDataAction.Ignore;
        },
        FSErrorReloadElseIgnore: e =>
        {
            using Page fsError = DialogPages.FSError(e, new(){ Button.Retry, Button.Ignore });
            fsError.MainInstruction = FSErrorLoadingCustomScriptMainInstruction;
            return Button.Retry.Equals(new Dialog(fsError).Show());
        },
        WarnOnUnhandledException: ex =>
        {
            Logs.UnhandledException.FormatWith(ex).Log(LogLevel.Critical);

            using Page unhandledException = new()
            {
                AllowHyperlinks = true,
                WindowTitle = UnhandledExceptionWindowTitle.FormatWith(AppInfo.Name),
                Icon = DialogIcon.Error,
                Content = UnhandledExceptionContent.FormatWith(ex.Message),
                Expander = new(ex.ToString()),
                Buttons = { Buttons.Exit },
            };
            unhandledException.HyperlinkClicked += async (s, e) =>
            {
                switch (e.Href)
                {
                    case "CopyDetails":
                        Clipboard.SetText(ex.ToString());
                        break;

                    case "ReportIssue":
                        (await SourceControlClient.Instance).NewIssueUrl.Open();
                        break;
                }
            };

            _ = new Dialog(unhandledException).Show();
        });
}