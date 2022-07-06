using Ookii.Dialogs.Wpf;

using static Scover.WinClean.Resources.UI.Buttons;

namespace Scover.WinClean.Presentation.Dialogs;

public class RetryIgnoreAbort : Dialog
{
    protected RetryIgnoreAbort()
    {
        Buttons.Add(new TaskDialogButton(Retry));
        Buttons.Add(new TaskDialogButton(Ignore));
        Buttons.Add(new TaskDialogButton(Abort));
    }

    public static RetryIgnoreAbort SystemRestoreDisabled => new()
    {
        MainIcon = TaskDialogIcon.Error,
        MainInstruction = Resources.UI.SystemProtectionDisabledDialog.MainInstruction,
        ExpandedInformation = new(Resources.UI.SystemProtectionDisabledDialog.ExpandedInformation),
        CollapsedControlText = Resources.UI.SystemProtectionDisabledDialog.CollapsedControlText,
        ExpandedControlText = Resources.UI.SystemProtectionDisabledDialog.ExpandedControlText
    };

    /// <returns>A <see cref="DialogResult"/> value based on the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new DialogResult ShowDialog() => Buttons.IndexOf(base.ShowDialog()) switch
    {
        0 => DialogResult.Retry,
        1 => DialogResult.Ignore,
        2 => DialogResult.Abort,
        _ => DialogResult.Closed
    };
}