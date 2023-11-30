using System.Globalization;

namespace Scover.WinClean.Services;

/// <summary>
/// ICU Message formatter.
/// </summary>
/// <remarks>Supports the <c>humanize</c> Time and Date format.</remarks>
public interface IMessageFormatter
{
    string Format(string message, IReadOnlyDictionary<string, object?> args);

    string Format(string message, CultureInfo culture, IReadOnlyDictionary<string, object?> args);
}