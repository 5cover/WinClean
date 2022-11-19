using Humanizer;
using Humanizer.Localisation;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;

namespace Scover.WinClean.Presentation;

public partial class ScriptExecutionWizard
{
    /// <returns>
    /// <see langword="true"/> if the user chose to create a restore point, <see langword="false"/> otherwise, and <see
    /// langword="null"/> if the user canceled or closed the dialog.
    /// </returns>
    private static bool? AskToCreateRestorePoint()
    {
        CommandLink yes = new()
        {
            Text = SystemRestorePoint.CommandLinkYes,
            Note = SystemRestorePoint.CommandLinkYesNote
        };
        using CommandLinkDialog restorePointDialog = new()
        {
            MainInstruction = SystemRestorePoint.MainInstruction,
            CustomMainIcon = Helpers.GetRestorePointIcon()!, // ! : null is ok
            CommandLinks =
            {
                yes,
                new()
                {
                    Text = SystemRestorePoint.CommandLinkNo
                }
            },
            DefaultCommandLink = yes,
            AreHyperlinksEnabled = true,
        };
        DialogResult result = restorePointDialog.ShowDialog();
        return result.WasClosed ? null : result.ClickedCommandLink == yes;
    }

    /// <returns>
    /// <see langword="true"/> if the user chose to enable system restore, <see langword="false"/> otherwise, and <see
    /// langword="null"/> if the user canceled or closed the dialog.
    /// </returns>
    private static bool? AskToEnableSystemRestore()
    {
        CommandLink enable = new()
        {
            Text = SystemProtectionDisabled.CommandLinkEnable,
            Note = SystemProtectionDisabled.CommandLinkEnableNote
        };

        using CommandLinkDialog enableSystemRestore = new()
        {
            MainInstruction = SystemProtectionDisabled.MainInstruction,
            MainIcon = TaskDialogIcon.Error,
            CommandLinks =
            {
                enable,
                new()
                {
                    Text = SystemProtectionDisabled.CommandLinkContinueAnyway,
                    Note = SystemProtectionDisabled.CommandLinkContinueAnywayNote
                }
            }
        };
        DialogResult result = enableSystemRestore.ShowDialog();
        return result.WasClosed ? null : result.ClickedCommandLink == enable;
    }

    /// <returns>
    /// <see langword="true"/> if the user chose to ignore the hung script, <see langword="false"/> if he chose to kill it.
    /// </returns>
    private static bool AskToIgnoreOrKillHungScript(string scriptName)
    {
        Logs.HungScript.FormatWith(scriptName, AppInfo.Settings.ScriptTimeout).Log(LogLevel.Warning);
        using TimeoutDialog hungScriptDialog = new(Button.EndTask, Button.Ignore)
        {
            AllowDialogCancellation = true,
            MainIcon = TaskDialogIcon.Warning,
            Content = Resources.UI.Dialogs.HungScriptDialogContent.FormatWith(scriptName, AppInfo.Settings.ScriptTimeout.Humanize(3, minUnit: TimeUnit.Second)),
            Timeout = 15.Seconds(),
            TimeoutButton = Button.Ignore
        };
        Button? clickedButton = hungScriptDialog.ShowDialog().ClickedButton;
        if (clickedButton == Button.EndTask)
        {
            Logs.HungScriptAborted.FormatWith(scriptName).Log(LogLevel.Info);
        }
        return clickedButton != Button.EndTask;
    }
}