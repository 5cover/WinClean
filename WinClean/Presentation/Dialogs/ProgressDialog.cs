using Humanizer;

using Ookii.Dialogs.Wpf;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>Dialog that tracks the progress of a lengthy operation. Has a single button for stopping or aborting the operation.</summary>
public sealed class ProgressDialog : Dialog
{
    /// <inheritdoc cref="Dialog(IEnumerable{Button})"/>
    public ProgressDialog(params Button[] buttons) : base(buttons)
    {
        Dlg.Timer += (_, e) =>
        {
            // Here ticks are actually milliseconds
            TimeSpan elapsed = e.TickCount.Milliseconds();
            Timer?.Invoke(this, new(elapsed));
            e.ResetTickCount = true;
        };
        Dlg.RaiseTimerEvent = true;

        SetConfirmation(Buttons.Keys.Single(), () =>
        {
            using Dialog confirmation = new(Button.Yes, Button.No)
            {
                MainIcon = TaskDialogIcon.Warning,
                Content = Resources.UI.Dialogs.ConfirmAbortOperationContent,
                DefaultButton = Button.No
            };
            return confirmation.ShowDialog().ClickedButton == Button.Yes;
        });
    }

    /// <summary>Event raised periodically while the dialog is displayed</summary>
    /// <remarks>The event is raised approximately every 200 milliseconds.</remarks>
    public event EventHandler<ProgressDialogTimerEventArgs>? Timer;

    #region Wrapped TaskDialog properties

    /// <inheritdoc cref="TaskDialog.ProgressBarMinimum"/>
    public int Maximum { get => Dlg.ProgressBarMinimum; set => Dlg.ProgressBarMaximum = value; }

    /// <inheritdoc cref="TaskDialog.ProgressBarMinimum"/>
    public int Minimum { get => Dlg.ProgressBarMinimum; set => Dlg.ProgressBarMinimum = value; }

    /// <inheritdoc cref="TaskDialog.ProgressBarState"/>
    public ProgressBarState State { get => Dlg.ProgressBarState; set => Dlg.ProgressBarState = value; }

    /// <inheritdoc cref="TaskDialog.ProgressBarStyle"/>
    public ProgressBarStyle Style { get => Dlg.ProgressBarStyle; set => Dlg.ProgressBarStyle = value; }

    /// <inheritdoc cref="TaskDialog.ProgressBarValue"/>
    public int Value { get => Dlg.ProgressBarValue; set => Dlg.ProgressBarValue = value; }

    #endregion Wrapped TaskDialog properties
}