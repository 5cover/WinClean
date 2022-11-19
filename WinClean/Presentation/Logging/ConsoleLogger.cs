namespace Scover.WinClean.Presentation.Logging;

public sealed class ConsoleLogger : Logger
{
    public override void ClearLogs() => Console.Clear();

    protected override void Log(LogEntry entry)
        => Console.Error.WriteLine($"[{entry.Level} at {entry.Date}] ({entry.CallFile}, line {entry.CallLine}): {entry.Message}");
}