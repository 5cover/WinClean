namespace Scover.WinClean.Operational;

/// <summary>Windows Registry Editor</summary>
public class Regedit : ScriptHost
{
    public override string Name => "Regedit";
    public override ExtensionGroup SupportedExtensions { get; } = new(".reg");

    protected override IncompleteArguments Arguments { get; } = new("/s {0}");

    protected override FileInfo Executable { get; } = new(Path.Join(Environment.GetEnvironmentVariable("SystemRoot"), "regedit.exe"));
}