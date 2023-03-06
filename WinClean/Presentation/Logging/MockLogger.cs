namespace Scover.WinClean.Presentation.Logging;

public sealed class MockLogger : Logger
{
    public override void ClearLogs()
    {
    }

    protected override void Log(LogEntry entry)
    {
    }
}