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
            update.Verification.Checked += (_, _) => ServiceProvider.Get<ISettings>().ShowUpdateDialog ^= true;
            _ = new Dialog(update).Show();
        },
        ScriptLoadError: (e, path) =>
        {
            Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);

            using Page deleteScriptPage = DialogPages.ConfirmScriptDeletion();

            Button deleteScriptButton = new(Buttons.DeleteScript);
            deleteScriptButton.Clicked += (s, e) => e.Cancel = !Button.Yes.Equals(new Dialog(deleteScriptPage).Show());

            using Page invalidScriptDataPage = DialogPages.ScriptLoadError(e, path, new(defaultItem: Button.TryAgain){ deleteScriptButton, Button.TryAgain, Button.Ignore });

            Dialog invalidScriptData = new(invalidScriptDataPage);
            var clicked = invalidScriptData.Show();

            if (deleteScriptButton.Equals(clicked))
            {
                Logs.ScriptRemoved.FormatWith(path).Log();
                return InvalidScriptDataAction.Remove;
            }

            return Button.TryAgain.Equals(clicked) ? InvalidScriptDataAction.Reload : InvalidScriptDataAction.Ignore;
        },
        FSErrorReloadElseIgnore: e =>
        {
            using Page fsError = DialogPages.FSError(e, new(){ Button.TryAgain, Button.Ignore });
            fsError.MainInstruction = WinClean.Resources.UI.Dialogs.FSErrorLoadingCustomScriptMainInstruction;
            return Button.TryAgain.Equals(new Dialog(fsError).Show());
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

            return Button.Ignore.Equals(new Dialog(unhandledException).Show());
        });
}