using Ookii.Dialogs.Wpf;

using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>A dialog with a timeout.</summary>
public sealed class TimeoutDialog : Dialog
{
    private TimeSpan _timeout;

    /// <summary>Initializes a new instance of the <see cref="TimeoutDialog"/> class.</summary>
    /// <inheritdoc/>
    public TimeoutDialog(IEnumerable<Button> buttons) : base(buttons)
    {
    }

    /// <inheritdoc cref="TimeoutDialog(IEnumerable{Button})"/>
    public TimeoutDialog(params Button[] buttons) : base(buttons)
    {
    }

    /// <summary>Gets or sets the timeout of this dialog.</summary>
    /// <remarks>
    /// When the dialog is shown, its progress bar will gradually fill until <see cref="Timeout"/> is reached, then <see
    /// cref="TimeoutButton"/> will be clicked.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The new property value is negative or zero.</exception>
    public TimeSpan Timeout
    {
        get => _timeout;
        init
        {
            _timeout = value;
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(value), DevException.ValueCannotBeNegative);
            }
            Dlg.ProgressBarStyle = ProgressBarStyle.ProgressBar;
            // Multiply by 0.9 so that the progress bar has the time to completely fill up before the dialog closes.
            Dlg.ProgressBarMaximum = Convert.ToInt32(_timeout.TotalMilliseconds * 0.9);
            Dlg.RaiseTimerEvent = true;
            Dlg.Timer += TimeoutTimer;
        }
    }

    /// <summary>
    /// Gets or sets the button that will be clicked when <see cref="Timeout"/> is reached. If <see langword="null"/>, the
    /// dialog will be closed.
    /// </summary>
    public Button? TimeoutButton { get; set; }

    private void TimeoutTimer(object? sender, TimerEventArgs e)
    {
        // TickCount is actually in milliseconds
        if (e.TickCount > Convert.ToInt32(_timeout.TotalMilliseconds))
        {
            if (TimeoutButton is null)
            {
                Close();
            }
            else
            {
                Buttons[TimeoutButton.Value].Click();
            }
            // The timeout was reached, so stop raising timer events, we don't need them anymore.
            Dlg.RaiseTimerEvent = false;
        }
        Dlg.ProgressBarValue = Math.Min(e.TickCount, Dlg.ProgressBarMaximum);
    }
}