using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>A dialog that supports custom buttons, timeouts and confirmations.</summary>
public class Dialog : IDisposable
{
    #region TaskDialog wrapped properties and events

    public event EventHandler Created { add => Dlg.Created += value; remove => Dlg.Created -= value; }

    public event EventHandler<ExpandButtonClickedEventArgs> ExpandButtonClicked { add => Dlg.ExpandButtonClicked += value; remove => Dlg.ExpandButtonClicked -= value; }

    public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked { add => Dlg.HyperlinkClicked += value; remove => Dlg.HyperlinkClicked -= value; }

    public event EventHandler VerificationClicked { add => Dlg.VerificationClicked += value; remove => Dlg.VerificationClicked -= value; }

    public bool AllowDialogCancellation { get => Dlg.AllowDialogCancellation; set => Dlg.AllowDialogCancellation = value; }
    public bool AreHyperlinksEnabled { get => Dlg.EnableHyperlinks; set => Dlg.EnableHyperlinks = value; }
    public string Content { get => Dlg.Content; set => Dlg.Content = value; }
    public System.Drawing.Icon CustomMainIcon { get => Dlg.CustomMainIcon; set => Dlg.CustomMainIcon = value; }
    public string ExpandedInformation { get => Dlg.ExpandedInformation; set => Dlg.ExpandedInformation = value; }
    public bool IsVerificationChecked { get => Dlg.IsVerificationChecked; set => Dlg.IsVerificationChecked = value; }
    public TaskDialogIcon MainIcon { get => Dlg.MainIcon; set => Dlg.MainIcon = value; }
    public string MainInstruction { get => Dlg.MainInstruction; set => Dlg.MainInstruction = value; }
    public bool ShowMinimizeBox { get => Dlg.MinimizeBox; set => Dlg.MinimizeBox = value; }
    public bool StartExpanded { get => Dlg.ExpandedByDefault; set => Dlg.ExpandedByDefault = value; }
    public string VerificationText { get => Dlg.VerificationText; set => Dlg.VerificationText = value; }

    #endregion TaskDialog wrapped properties and events

    private readonly IDictionary<TaskDialogButton, Func<bool>> _confirmations = new Dictionary<TaskDialogButton, Func<bool>>();
    private Button? _defaultButton;

    /// <summary>Initializes a new instance of the <see cref="Dialog"/> class.</summary>
    /// <remarks>
    /// Sets the following properties on the wrapped <see cref="TaskDialog"/>: <br><see cref="TaskDialog.WindowTitle"/> to <see
    /// cref="AppInfo.Name"/>;</br><br><see cref="TaskDialog.CenterParent"/> to <see langword="true"/>.</br>
    /// </remarks>
    /// <param name="buttons">The buttons to add to the dialog.</param>
    /// <exception cref="ArgumentException">The same button is specified multiple times.</exception>
    public Dialog(IEnumerable<Button> buttons)
    {
        Dlg.WindowTitle = AppInfo.Name;

        foreach (Button button in buttons.OrderBy(b => b))
        {
            TaskDialogButton tdButton = new(button.GetText());
            Buttons[button] = tdButton;
            Dlg.Buttons.Add(tdButton);
        }

        Dlg.ButtonClicked += (_, e) =>
        {
            if (!IsClosed && _confirmations.TryGetValue((TaskDialogButton)e.Item, out var confirmation))
            {
                e.Cancel = !confirmation();
            }
        };
    }

    /// <inheritdoc cref="Dialog(IEnumerable{Button})"/>
    public Dialog(params Button[] buttons) : this((IEnumerable<Button>)buttons)
    {
    }

    /// <summary>Gets or sets the the default button. Set to <see langword="null"/> to disable this feature.</summary>
    /// <value>The default button, or <see langword="null"/> if this feature is disabled.</value>
    /// <exception cref="KeyNotFoundException">The new property value is not a button of this instance.</exception>
    public Button? DefaultButton
    {
        get => _defaultButton;
        set
        {
            if (_defaultButton is not null)
            {
                // Remove the old default button
                Buttons[_defaultButton.Value].Default = false;
            }

            if (value is not null)
            {
                // Set the new default button
                Buttons[value.Value].Default = true;
            }

            _defaultButton = value;
        }
    }

    /// <summary>Gets the buttons of this dialog.</summary>
    protected IDictionary<Button, TaskDialogButton> Buttons { get; } = new Dictionary<Button, TaskDialogButton>();

    /// <summary>Gets the wrapped <see cref="TaskDialog"/> object.</summary>
    protected TaskDialog Dlg { get; } = new();

    /// <summary>Gets or sets whether the dialog is closed or closing.</summary>
    /// <value>
    /// If <see langword="true"/>, confirmation delegates will not be called, and <see cref="DialogResult"/> objects returned by
    /// <see cref="Show"/> and <see cref="ShowDialog"/> will have their property <see cref="DialogResult.WasClosed"/> set to
    /// <see langword="true"/>.
    /// </value>
    protected bool IsClosed { get; set; }

    /// <summary>Closes this dialog.</summary>
    /// <remarks>
    /// Does not call any confirmation delegates. <see cref="DialogResult"/> objects returned by <see cref="Show"/> and <see
    /// cref="ShowDialog"/> will have their property <see cref="DialogResult.WasClosed"/> set to <see langword="true"/>.
    /// </remarks>
    public void Close()
    {
        IsClosed = true;
        // click any button, it doesnt matter.
        Dlg.Buttons.First().Click();
        IsClosed = false;
    }

    public void Dispose()
    {
        Dlg.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Sets the confirmation action for a button.</summary>
    /// <param name="button">The button to set a confirmation for.</param>
    /// <param name="confirmation">
    /// A delegate that returns <see langword="true"/> if the button action is confirmed; otherwise <see langword="false"/>.
    /// </param>
    /// <exception cref="KeyNotFoundException"><paramref name="button"/> is not a button of this instance.</exception>
    public void SetConfirmation(Button button, Func<bool> confirmation)
    {
        _confirmations[Buttons[button]] = confirmation;
    }

    /// <inheritdoc cref="TaskDialog.Show"/>
    public DialogResult Show() => GetResult(Dlg.Show());

    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public DialogResult ShowDialog() => GetResult(Dlg.ShowDialog());

    protected virtual DialogResult GetResult(TaskDialogButton? clickedButton)
    {
        return new(Buttons.Keys.SingleOrDefault(b => ReferenceEquals(Buttons[b], clickedButton)),
                   null);
    }
}