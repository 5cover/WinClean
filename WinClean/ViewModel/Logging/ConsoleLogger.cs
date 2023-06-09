using Optional;

using static System.ConsoleColor;

namespace Scover.WinClean.ViewModel.Logging;

public sealed class ConsoleLogger : Logger
{
    public override Task ClearLogs()
    {
        Console.Clear();
        return Task.CompletedTask;
    }

    protected override void Log(LogEntry entry)
    {
        var (background, foreground) = GetColor(entry.Level);
        background.MatchSome(color => Console.BackgroundColor = color);
        foreground.MatchSome(color => Console.ForegroundColor = color);

        Console.Error.WriteLine($"[{entry.Date}] ({entry.CallFile}, line {entry.CallLine}): {entry.Message}");
        Console.ResetColor();
    }

    private static (Option<ConsoleColor>, Option<ConsoleColor>) Color(ConsoleColor? background = null, ConsoleColor? foreground = null)
        => (background is { } b ? b.Some() : Option.None<ConsoleColor>(),
            foreground is { } f ? f.Some() : Option.None<ConsoleColor>());

    private static (Option<ConsoleColor>, Option<ConsoleColor>) GetColor(LogLevel lvl) => lvl switch
    {
        LogLevel.Verbose => Color(),
        LogLevel.Info => Color(foreground: Blue),
        LogLevel.Warning => Color(foreground: Yellow),
        LogLevel.Error => Color(foreground: Red),
        LogLevel.Critical => Color(background: Red, foreground: White),
        _ => throw lvl.NewInvalidEnumArgumentException()
    };
}