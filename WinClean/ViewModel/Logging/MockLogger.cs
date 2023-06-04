namespace Scover.WinClean.ViewModel.Logging;

public sealed class MockLogger : Logger
{
    public override Task ClearLogs() => Task.CompletedTask;

    protected override void Log(LogEntry entry)
    {
    }
}