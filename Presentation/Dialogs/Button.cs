using System.Globalization;

using Scover.WinClean.Resources.UI;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>Represents a button displayed in a <see cref="Dialog"/>.</summary>
/// <remarks>
/// The order of the values describes the order of the button on the <see cref="Dialog"/>, from left (top) to right (bottom).
/// <br>For example, in a dialog, <see cref="OK"/> will be shown to the left of <see cref="Cancel"/> because 5 &lt; 11.</br>
/// </remarks>
public enum Button
{
    Stop,
    DeleteScript,
    EndTask,
    Exit,
    Restart,
    OK,
    Retry,
    Continue,
    Yes,
    No,
    Ignore,
    Cancel
}

public static class ButtonExtensions
{
    public static string GetText(this Button button)
        => (Buttons.ResourceManager
                   .GetString(Enum.GetName(button) ?? throw new ArgumentException($"The enumeration value ('{button}') does not have a name.", nameof(button)), CultureInfo.CurrentUICulture)
                   ?? string.Empty)
                   .Replace('_', '&'); // TaskDialog uses '&' for access key chars.
}