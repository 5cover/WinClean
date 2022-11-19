namespace Scover.WinClean.Presentation.Dialogs;

public sealed record DialogResult(Button? ClickedButton, CommandLink? ClickedCommandLink)
{
    /// <summary>Gets whether the dialog was closed using the title bar close action.</summary>
    public bool WasClosed => ClickedButton is null && ClickedCommandLink is null;
}