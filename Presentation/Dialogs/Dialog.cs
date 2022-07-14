using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>A dialog that supports custom buttons, timeouts and confirmations.</summary>
public class Dialog : TaskDialog
{
    private readonly List<Button> _buttons;

    private readonly Dictionary<int, Func<bool>> _confirmations = new();

    private int? _defaultButtonIndex;

    private TimeSpan _timeout = TimeSpan.Zero;

    private TaskDialogButton? _timeoutButton;

    private ProgressBarInfo _timeoutDisabledProgressBar;
    private bool _timeoutDisabledRaiseTimerEvent;

    /// <summary>Initializes a new instance of the <see cref="Dialog"/> class.</summary>
    /// <param name="buttons">The buttons to add to the dialog.</param>
    /// <remarks>
    /// Also sets the following properties: <br><see cref="TaskDialog.WindowTitle"/> to <see cref="AppInfo.Name"/>;</br><br><see
    /// cref="TaskDialog.CenterParent"/> to <see langword="true"/>.</br>
    /// </remarks>
    public Dialog(IEnumerable<Button> buttons)
    {
        _timeoutDisabledProgressBar = new(this);

        WindowTitle = AppInfo.Name;
        CenterParent = true;

        _buttons = buttons.ToList();
        _buttons.Sort();
        foreach (Button button in _buttons)
        {
            Buttons.Add(new(button.GetText()));
        }

        ButtonClicked += (_, e) =>
        {
            int clickedButtonIndex = Buttons.IndexOf(e.Item as TaskDialogButton);
            if (!IsClosed && clickedButtonIndex != -1) e.Cancel = !_confirmations.GetValueOrDefault(clickedButtonIndex)?.Invoke() ?? false;
        };
    }

    /// <inheritdoc cref="Dialog(IEnumerable{Button})"/>
    public Dialog(params Button[] buttons) : this((IEnumerable<Button>)buttons)
    {
    }

    /// <summary>Gets or sets the the default button. Set to <see langword="null"/> to disable this feature.</summary>
    /// <value>The default button, or <see langword="null"/> if this feature is disabled.</value>
    /// <exception cref="ArgumentException">The new property value is not a button of this instance.</exception>
    public Button? DefaultButton
    {
        get => _defaultButtonIndex is null ? null : _buttons[_defaultButtonIndex.Value];
        set
        {
            if (_defaultButtonIndex is not null)
            {
                Buttons[_defaultButtonIndex.Value].Default = false;
            }

            if (value is not null)
            {
                int newDefaultButtonIndex = _buttons.IndexOf(value.Value);
                if (newDefaultButtonIndex == -1) throw new ArgumentException(DevException.NotAButtonOfInstance, nameof(value));
                _defaultButtonIndex = newDefaultButtonIndex;
                Buttons[_defaultButtonIndex.Value].Default = true;
            }
        }
    }

    /// <summary>Gets or sets the timeout of this dialog. Specify <see langword="TimeSpan.Zero"/> to disable this feature.</summary>
    /// <remarks>
    /// When the timeout is enabled, the dialog's progress bar will gradually fill up and when <see cref="Timeout"/> is reached,
    /// the <see cref="TimeoutButton"/> is clicked.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The new property value is negative.</exception>
    /// <value>The timeout of this dialog, or <see cref="TimeSpan.Zero"/> if this feature is disabled.</value>
    public TimeSpan Timeout
    {
        get => _timeout;
        set
        {
            if (value == TimeSpan.Zero)
            {
                _timeoutDisabledProgressBar.ApplyTo(this);
                RaiseTimerEvent = _timeoutDisabledRaiseTimerEvent;
                Timer -= TimeoutTimer;
            }
            else
            {
                _timeout = value;
                if (value < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(value), DevException.ValueCannotBeNegative);

                _timeoutDisabledProgressBar = new(this);
                _timeoutDisabledRaiseTimerEvent = RaiseTimerEvent;

                ProgressBarStyle = ProgressBarStyle.ProgressBar;
                ProgressBarMaximum = Convert.ToInt32(_timeout.TotalMilliseconds);
                RaiseTimerEvent = true;
                Timer += TimeoutTimer;
            }
        }
    }

    public Button TimeoutButton
    {
        get => _buttons[Buttons.IndexOf(_timeoutButton)];
        set => _timeoutButton = GetButton(value);
    }

    /// <summary>Gets or sets whether the dialog is closed or closing.</summary>
    /// <value>
    /// If <see langword="true"/>, confirmation delegates will not be shown, and <see cref="Show"/> and <see cref="ShowDialog"/>
    /// will return <see langword="null"/>.
    /// </value>
    /// <remarks>Default value is <see langword="false"/>.</remarks>
    protected bool IsClosed { get; set; }

    /// <summary>Sets the confirmation action for a button.</summary>
    /// <param name="button">The button to set a confirmation for.</param>
    /// <param name="confirmation">
    /// A delegate that returns <see langword="true"/> if the button action is confirmed; otherwise <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="button"/> is not a button of this instance.</exception>
    public void SetConfirmation(Button button, Func<bool> confirmation)
    {
        int buttonIndex = _buttons.IndexOf(button);
        if (buttonIndex == -1) throw new ArgumentException(DevException.NotAButtonOfInstance, nameof(button));
        _confirmations[buttonIndex] = confirmation;
    }

    /// <inheritdoc cref="TaskDialog.Show"/>
    public new Button? Show() => GetResult(base.Show());

    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new Button? ShowDialog() => GetResult(base.ShowDialog());

    protected TaskDialogButton GetButton(Button button)
    {
        int index = _buttons.IndexOf(button);
        return index == -1 ? throw new ArgumentException(DevException.NotAButtonOfInstance, nameof(button)) : Buttons[index];
    }

    private Button? GetResult(TaskDialogButton clickedButton)
    {
        int clickedButtonIndex = Buttons.IndexOf(clickedButton);
        Button? result = clickedButtonIndex == -1 ? null : _buttons[clickedButtonIndex];
        return IsClosed ? null : result;
    }

    private void TimeoutTimer(object? sender, TimerEventArgs e)
    {
        // TickCount is actually in milliseconds
        if (e.TickCount > ProgressBarMaximum)
        {
            _timeoutButton?.Click();
            // The timeout was reached, so stop raising timer events, we don't need them anymore.
            RaiseTimerEvent = false;
        }
        else
        {
            ProgressBarValue = e.TickCount;
        }
    }

    private readonly struct ProgressBarInfo
    {
        private readonly int _max;
        private readonly int _min;
        private readonly int _speed;
        private readonly ProgressBarState _state;
        private readonly ProgressBarStyle _style;
        private readonly int _value;

        public ProgressBarInfo(TaskDialog dialog)
        {
            _min = dialog.ProgressBarMinimum;
            _max = dialog.ProgressBarMaximum;
            _value = dialog.ProgressBarValue;
            _speed = dialog.ProgressBarMarqueeAnimationSpeed;
            _state = dialog.ProgressBarState;
            _style = dialog.ProgressBarStyle;
        }

        public void ApplyTo(TaskDialog dialog)
        {
            dialog.ProgressBarMinimum = _min;
            dialog.ProgressBarMaximum = _max;
            dialog.ProgressBarValue = _value;
            dialog.ProgressBarMarqueeAnimationSpeed = _speed;
            dialog.ProgressBarState = _state;
            dialog.ProgressBarStyle = _style;
        }
    }
}