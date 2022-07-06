using Ookii.Dialogs.Wpf;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>Represents a dialog with custum buttons.</summary>
public class CustomDialog : Dialog
{
    public CustomDialog(params string[] buttons)
    {
        foreach (string buttonText in buttons)
        {
            Buttons.Add(new(buttonText));
        }
    }

    /// <returns>The text of the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new string ShowDialog() => base.ShowDialog().Text;
}