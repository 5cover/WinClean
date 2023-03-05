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
        (e, path) =>
        {
            Logger.Log(Logs.ScriptLoadError.FormatWith(path, e), LogLevel.Error);

            using Page deleteScriptPage = DialogPageFactory.DeleteScript();
            Button deleteScriptButton = new(Buttons.DeleteScript);
            deleteScriptButton.Clicked += (s, e) => e.Cancel = Button.Yes.Equals(new Dialog(deleteScriptPage).Show());

            using Page invalidScriptDataPage = DialogPageFactory.InvalidScriptData(e, path, new(defaultItem: Button.Retry){ deleteScriptButton, Button.Retry, Button.Ignore });

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
            using Page fsError = DialogPageFactory.FSError(e, verb, info, new(){ Button.Retry, Button.Ignore });
            fsError.MainInstruction = FSErrorLoadingCustomScriptMainInstruction;
            return Button.Retry.Equals(new Dialog(fsError).Show());
        },
        ex =>
        {
            Logger.Log(Logs.UnhandledException.FormatWith(ex), LogLevel.Critical);

            using Page unhandledException = new()
            {
                AllowHyperlinks = true,
                WindowTitle = UnhandledExceptionWindowTitle.FormatWith(AppMetadata.Name),
                Icon = DialogIcon.Error,
                Content = UnhandledExceptionDialogContent.FormatWith(ex.Message),
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
                        (await SourceControlClient.Instance.GetNewIssueUrl()).Open();
                        break;
                }
            };

            _ = new Dialog(unhandledException).Show();
        });
}