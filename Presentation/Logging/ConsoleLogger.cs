namespace Scover.WinClean.Presentation.Logging;

public sealed class ConsoleLogger : Logger
{
    protected override void Log(LogEntry e)
    {
        Console.Error.WriteLine($"[{e.Level} at {e.Date}] ({e.CallFile}, line {e.CallLine}): {e.Message}");
    }
}