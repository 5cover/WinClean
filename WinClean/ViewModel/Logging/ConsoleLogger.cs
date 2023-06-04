namespace Scover.WinClean.ViewModel.Logging;

public sealed class ConsoleLogger : Logger
{
    public override Task ClearLogs()
    {
        Console.Clear();
        return Task.CompletedTask;
    }

    protected override void Log(LogEntry entry)
        => Console.Error.WriteLine($"[{entry.Level} at {entry.Date}] ({entry.CallFile}, line {entry.CallLine}): {entry.Message}");
}