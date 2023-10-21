using System.Globalization;

using Jeffijoe.MessageFormat;

namespace Scover.WinClean.Services;

public sealed class MessageFormatter : IMessageFormatter
{
    private const string HumanizeStyle = "humanize";

    private static readonly Jeffijoe.MessageFormat.MessageFormatter _msgFormatter = new(customValueFormatter: HumanizerValueFormatter.Instance);

    public string Format(string message, IReadOnlyDictionary<string, object?> args) => Format(message, CultureInfo.CurrentCulture, args);

    public string Format(string message, CultureInfo culture, IReadOnlyDictionary<string, object?> args)
    {
        _msgFormatter.Locale = culture.Name;
        return _msgFormatter.FormatMessage(message, args);
    }

    /// <summary>
    /// Custom MessageFormatter.NET formatter for Humanizer interoperability with dates, times and durations.
    /// </summary>
    private sealed class HumanizerValueFormatter : CustomValueFormatter
    {
        private HumanizerValueFormatter()
        {
        }

        public static HumanizerValueFormatter Instance { get; } = new();

        public override bool TryFormatDate(CultureInfo culture, object? value, string? style, out string? formatted)
            // MessageFormatter.NET doesn't do type checking so value could be of any type
            => (formatted = value switch
            {
                _ when style != HumanizeStyle => null,
                DateTime dt => dt.Humanize(culture: culture),
                DateOnly d => d.Humanize(culture: culture),
                DateTimeOffset dto => dto.Humanize(culture: culture),
                TimeOnly t => t.Humanize(culture: culture),
                _ => null
            }) is not null;

        public override bool TryFormatTime(CultureInfo culture, object? value, string? style, out string? formatted)
            => (formatted = value switch
            {
                _ when style != HumanizeStyle => null,
                TimeSpan d => d.HumanizeToSeconds(),
                _ => null
            }) is not null;
    }
}