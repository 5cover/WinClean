using Ookii.Dialogs.Wpf;

using Scover.WinClean.Presentation.Dialogs;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class WarningDialog : Dialog
{
    public WarningDialog() : base(Button.Continue, Button.Cancel)
    {
        AllowDialogCancellation = true;
        MainInstruction = Resources.UI.WarningDialog.MainInstruction;
        Content = Resources.UI.WarningDialog.Content;
        MainIcon = TaskDialogIcon.Warning;
        VerificationText = Resources.UI.WarningDialog.VerificationText;

        Buttons[0].Enabled = false;

        VerificationClicked += (_, _)
            => Buttons[0].Enabled = IsVerificationChecked;
    }
}