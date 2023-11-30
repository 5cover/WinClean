namespace Scover.WinClean.ViewModel.Logging;

public static partial class Logging
{
    private sealed class MockLogger : Logger
    {
        public override Task ClearLogsAsync() => Task.CompletedTask;

        protected override void Log(LogEntry entry)
        {
        }
    }
}