namespace Scover.WinClean.Model;

public sealed record ExecutionResult(int ExitCode, bool Succeeded, TimeSpan ExecutionTime);