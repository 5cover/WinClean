using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>A dialog that supports custom buttons, timeouts and confirmations.</summary>
/// <remarks>Wraps <see cref="TaskDialog"/>.</remarks>
public class Dialog : IDisposable
{
    private readonly Dictionary<TaskDialogButton, Func<bool>> _confirmations = new();
    private Button? _defaultButton;

    /// <remarks>
    /// Sets the following properties on the wrapped <see cref="TaskDialog"/>: <br><see cref="TaskDialog.WindowTitle"/> to <see
    /// cref="AppInfo.Name"/>;</br><br><see cref="TaskDialog.CenterParent"/> to <see langword="true"/>.</br>
    /// </remarks>
    /// <param name="buttons">The buttons to add to the dialog.</param>
    /// <exception cref="ArgumentException">The same button is specified multiple times.</exception>
    public Dialog(IEnumerable<Button> buttons)
    {
        Dlg.WindowTitle = AppInfo.Name;
        Dlg.CenterParent = true;

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
        if (Dlg.Buttons.Any())
        {
            // click any button, it doesnt matter.
            Dlg.Buttons.First().Click();
        }
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
    public void SetConfirmation(Button button, Func<bool> confirmation) => _confirmations[Buttons[button]] = confirmation;

    /// <returns>A <see cref="DialogResult"/> containing the button or the command link that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.Show"/>
    public DialogResult Show() => GetResult(Dlg.Show());

    /// <inheritdoc cref="Show" path="/returns"/>
    /// <inheritdoc cref="TaskDialog.ShowDialog()"/>
    public DialogResult ShowDialog() => GetResult(Dlg.ShowDialog());

    /// <summary>Gets the dialog result information from the button that was clicked.</summary>
    /// <param name="clickedButton">The button that was clicked to close the dialog.</param>
    /// <returns>A new <see cref="DialogResult"/> object that containes the dialog result information.</returns>
    protected virtual DialogResult GetResult(TaskDialogButton? clickedButton)
    {
        Button? clicked;
        try
        {
            clicked = Buttons.Keys.Single(b => Buttons[b] == clickedButton);
        }
        catch (InvalidOperationException)
        {
            clicked = null;
        }
        return new(IsClosed ? null : clicked, null);
    }

    #region TaskDialog wrapped properties and events

    ///<summary>Event raised when the dialog has been created.</summary>
    ///<remarks>This event is raised once after calling <see cref="Show"/> or <see cref="ShowDialog"/>, after the dialog is created and before it is displayed.</remarks>
    public event EventHandler Created { add => Dlg.Created += value; remove => Dlg.Created -= value; }

    /// <summary>Event raised when the user clicks the expand button on the dialog.</summary>
    /// <remarks>
    /// The <see cref="ExpandButtonClickedEventArgs.Expanded"/> property indicates if the expanded information is visible or not
    /// after the click.
    /// </remarks>
    public event EventHandler<ExpandButtonClickedEventArgs> ExpandButtonClicked { add => Dlg.ExpandButtonClicked += value; remove => Dlg.ExpandButtonClicked -= value; }

    /// <summary>Event raised when the user clicks a hyperlink.</summary>
    public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked { add => Dlg.HyperlinkClicked += value; remove => Dlg.HyperlinkClicked -= value; }

    /// <summary>Event raised when the user clicks the verification check box.</summary>
    public event EventHandler VerificationClicked { add => Dlg.VerificationClicked += value; remove => Dlg.VerificationClicked -= value; }

    /// <summary>
    /// Gets or sets a value that indicates that the dialog should be able to be closed using Alt-F4, Escape and the title bar's
    /// close button even if no cancel button is specified.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the dialog can be closed using Alt-F4, Escape and the title bar's close button even if no
    /// cancel button is specified; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool AllowDialogCancellation { get => Dlg.AllowDialogCancellation; set => Dlg.AllowDialogCancellation = value; }

    /// <summary>
    /// Gets or sets a value that indicates whether hyperlinks are allowed for the <see cref="Content"/>, <see
    /// cref="ExpandedInformation"/> and <see cref="Footer"/> properties.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="true"/>, the <see cref="Content"/>, <see cref="ExpandedInformation"/> and <see
    /// cref="Footer"/> properties can use hyperlinks in the following form: <c>&lt;A HREF="executablestring"&gt;Hyperlink
    /// Text&lt;/A&gt;</c><note>Enabling hyperlinks when using content from an unsafe source may cause security
    /// vulnerabilities.</note> Dialogs will not actually execute hyperlinks. To take action when the user presses a hyperlink,
    /// handle the <see cref="HyperlinkClicked"/> event.
    /// </remarks>
    /// <value>
    /// <see langword="true"/> when hyperlinks are allowed for the <see cref="Content"/>, <see cref="ExpandedInformation"/> and
    /// <see cref="Footer"/> properties; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool AreHyperlinksEnabled { get => Dlg.EnableHyperlinks; set => Dlg.EnableHyperlinks = value; }

    /// <inheritdoc cref="TaskDialog.Content"/>
    public string Content { get => Dlg.Content; set => Dlg.Content = value; }

    /// <summary>Gets or sets a custom icon to display in the dialog.</summary>
    /// <value>
    /// An <see cref="System.Drawing.Icon"/> that represents the icon to display in the main content area of the dialog, or <see
    /// langword="null"/> if no custom icon is used. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>This property is ignored if the <see cref="MainIcon"/> property has a value other than <see cref="TaskDialogIcon.Custom"/>.</remarks>
    public System.Drawing.Icon CustomMainIcon { get => Dlg.CustomMainIcon; set => Dlg.CustomMainIcon = value; }

    /// <summary>Gets or sets additional information to be displayed on the dialog.</summary>
    /// <remarks>
    /// When this property is not an empty string (""), a control is shown on the task dialog that allows the user to expand and
    /// collapse the text specified in this property. The text is collapsed by default unless <see cref="StartExpanded"/> is set
    /// to <see langword="true"/>. The expanded text is shown in the main content area of the dialog.
    /// </remarks>
    /// <value>Additional information to be displayed on the dialog. The default value is an empty string ("").</value>
    public string ExpandedInformation { get => Dlg.ExpandedInformation; set => Dlg.ExpandedInformation = value; }

    /// <summary>Gets or sets the text to be used in the footer area of the dialog.</summary>
    /// <value>
    /// The text to be used in the footer area of the dialog, or an empty string ("") if the footer area is not displayed. The
    /// default value is an empty string ("").
    /// </value>
    public string Footer { get => Dlg.Footer; set => Dlg.Footer = value; }

    /// <summary>Gets or sets a value that indicates whether the verification checkbox is checked ot not.</summary>
    /// <remarks>
    /// Set this property before displaying the dialog to determine the initial state of the check box. Use this property after
    /// displaying the dialog to determine whether the check box was checked when the user closed the dialog. This property is
    /// only used if <see cref="VerificationText"/> is not an empty string ("").
    /// </remarks>
    /// <value><see langword="true"/> if the verficiation checkbox is checked; otherwise, <see langword="false"/>.</value>
    public bool IsVerificationChecked { get => Dlg.IsVerificationChecked; set => Dlg.IsVerificationChecked = value; }

    /// <summary>Gets or sets the icon to display in the dialog.</summary>
    /// <value>
    /// A <see cref="TaskDialogIcon"/> that indicates the icon to display in the main content area of the dialog. The default is
    /// <see cref="TaskDialogIcon.Custom"/>.
    /// </value>
    /// <remarks>
    /// When this property is set to <see cref="TaskDialogIcon.Custom"/>, use the <see cref="CustomMainIcon"/> property to
    /// specify the icon to use.
    /// </remarks>
    public TaskDialogIcon MainIcon { get => Dlg.MainIcon; set => Dlg.MainIcon = value; }

    /// <summary>Gets or sets the dialog's main instruction.</summary>
    /// <remarks>
    /// The main instruction of a dialog will be displayed in a larger font and a different color than the other text of the dialog.
    /// </remarks>
    /// <value>The dialog's main instruction. The default is an empty string ("").</value>
    public string MainInstruction { get => Dlg.MainInstruction; set => Dlg.MainInstruction = value; }

    /// <inheritdoc cref="TaskDialog.MinimizeBox"/>
    public bool ShowMinimizeBox { get => Dlg.MinimizeBox; set => Dlg.MinimizeBox = value; }

    /// <inheritdoc cref="TaskDialog.ExpandedByDefault"/>
    public bool StartExpanded { get => Dlg.ExpandedByDefault; set => Dlg.ExpandedByDefault = value; }

    /// <inheritdoc cref="TaskDialog.VerificationText"/>
    public string VerificationText { get => Dlg.VerificationText; set => Dlg.VerificationText = value; }

    #endregion TaskDialog wrapped properties and events
}