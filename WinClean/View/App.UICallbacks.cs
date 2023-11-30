using System.Diagnostics;
using System.Windows;

using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.View;

public partial class App
{
    private static readonly Callbacks uiCallbacks = new(
        NotifyUpdateAvailable: latestVersionName =>
        {
            using Page update = new()
            {
                AllowHyperlinks = true,
                IsCancelable = true,
                MainInstruction = Update.MainInstruction,
                Sizing = Sizing.Content,
                Icon = DialogIcon.Information,
                Content = Update.Content.FormatWith(latestVersionName),
                Verification = new(Update.Verification),
            };
            update.HyperlinkClicked += (_, _) => ServiceProvider.Get<ISettings>().LatestVersionUrl.Open();
            update.Verification.Checked += (_, _) => ServiceProvider.Get<ISettings>().ShowUpdateDialog ^= true;
            _ = new Dialog(update).ShowDialog();
        },
        ScriptLoadError: (e, path) =>
        {
            Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);

            Button deleteScriptButton = new(Buttons.DeleteScript);
            deleteScriptButton.Clicked += (_, args) => args.Cancel = !DialogFactory.ShowConfirmation(DialogFactory.MakeConfirmScriptDeletion);

            using Page invalidScriptDataPage = DialogFactory.MakeScriptLoadError(e, path, new(defaultItem: Button.TryAgain) { deleteScriptButton, Button.TryAgain, Button.Ignore });

            Dialog invalidScriptData = new(invalidScriptDataPage);
            var clicked = invalidScriptData.ShowDialog();

            if (deleteScriptButton.Equals(clicked))
            {
                Logs.ScriptDeleted.FormatWith(path).Log();
                return InvalidScriptDataAction.Delete;
            }

            return Button.TryAgain.Equals(clicked) ? InvalidScriptDataAction.Reload : InvalidScriptDataAction.Ignore;
        },
        FSErrorReloadElseIgnore: e =>
        {
            using Page fsError = DialogFactory.MakeFSError(e, new() { Button.TryAgain, Button.Ignore });
            fsError.MainInstruction = WinClean.Resources.UI.Dialogs.FSErrorLoadingCustomScriptMainInstruction;
            return Button.TryAgain.Equals(new Dialog(fsError).ShowDialog());
        },
        WarnOnUnhandledException: ex =>
        {
            Logs.UnhandledException.FormatWith(ex).Log(LogLevel.Critical);

            using Page unhandledException = new()
            {
                AllowHyperlinks = true,
                WindowTitle = WinClean.Resources.UI.Dialogs.UnhandledExceptionWindowTitle.FormatWith(ServiceProvider.Get<IApplicationInfo>().Name),
                Icon = DialogIcon.Error,
                Content = WinClean.Resources.UI.Dialogs.UnhandledExceptionContent.FormatWith(ex.Message),
                Expander = new(ex.ToString()),
                Buttons = { Button.Ignore, Buttons.Exit },
            };
            unhandledException.HyperlinkClicked += (_, e) =>
            {
                // Dialog href links are used as link identifiers
                switch (e.Href)
                {
                    case "CopyDetails":
                        Clipboard.SetText(ex.ToString());
                        break;
                    case "ReportIssue":
                        ServiceProvider.Get<ISettings>().NewIssueUrl.Open();
                        break;
                    default:
                        Debug.Fail("Unknown href");
                        break;
                }
            };

            return Button.Ignore.Equals(new Dialog(unhandledException).ShowDialog());
        });
}