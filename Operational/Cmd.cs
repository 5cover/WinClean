namespace Scover.WinClean.Operational;

/// <summary>The Windows Command Line interpreter (cmd.exe) script host.</summary>
public class Cmd : ScriptHost
{
    public override string Name => "Cmd";

    public override ExtensionGroup SupportedExtensions { get; } = new(".cmd", ".bat");

    protected override IncompleteArguments Arguments { get; } = new("/d /c \"{0}\"");

    protected override FileInfo Executable { get; } = new(Environment.GetEnvironmentVariable("comspec", EnvironmentVariableTarget.Machine)!); // ! : comspecs exists natively on windows
}