namespace Scover.WinClean.Model;

public sealed record ExecutionResult(int ExitCode, TimeSpan ExecutionTime)
{
    public bool Succeeded => ExitCode == 0;
}