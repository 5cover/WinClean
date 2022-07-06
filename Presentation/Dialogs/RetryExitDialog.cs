using Ookii.Dialogs.Wpf;

using static Scover.WinClean.Resources.UI.Buttons;

namespace Scover.WinClean.Presentation.Dialogs;

public class RetryExitDialog : Dialog
{
    public RetryExitDialog()
    {
        Buttons.Add(new(Retry));
        Buttons.Add(new(Exit));
    }

    /// <returns>A <see cref="DialogResult"/> value based on the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new DialogResult ShowDialog() => Buttons.IndexOf(base.ShowDialog()) switch
    {
        0 => DialogResult.Retry,
        1 => DialogResult.Exit,
        _ => DialogResult.Closed
    };
}