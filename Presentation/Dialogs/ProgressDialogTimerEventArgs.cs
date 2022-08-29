namespace Scover.WinClean.Presentation.Dialogs;

public class ProgressDialogTimerEventArgs : EventArgs
{
    public ProgressDialogTimerEventArgs(TimeSpan elapsed) => Elapsed = elapsed;

    /// <summary>Gets the elapsed time since the last time the event was raised.</summary>
    public TimeSpan Elapsed { get; }
}