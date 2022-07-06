using Ookii.Dialogs.Wpf;

using Scover.WinClean.Presentation.Dialogs;

using static Scover.WinClean.Resources.UI.Buttons;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class WarningDialog : Dialog
{
    public WarningDialog()
    {
        AllowDialogCancellation = true;
        WindowTitle = App.Name;
        MainInstruction = Resources.UI.WarningDialog.MainInstruction;
        Content = Resources.UI.WarningDialog.Content;
        MainIcon = TaskDialogIcon.Warning;
        VerificationText = Resources.UI.WarningDialog.VerificationText;

        Buttons.Add(new(Cancel));
        Buttons.Add(new(Continue));

        Buttons[1].Enabled = false;

        VerificationClicked += (_, _)
            => Buttons[1].Enabled = IsVerificationChecked;
    }

    /// <returns>A <see cref="DialogResult"/> value based on the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new DialogResult ShowDialog() => Buttons.IndexOf(base.ShowDialog()) switch
    {
        0 => DialogResult.Cancel,
        1 => DialogResult.Continue,
        _ => DialogResult.Closed
    };
}