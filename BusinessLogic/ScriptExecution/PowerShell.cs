namespace Scover.WinClean.BusinessLogic.ScriptExecution;

/// <summary>The Windows PowerShell script host.</summary>
public class PowerShell : ScriptHost
{
    private static readonly string _path = Path.Join(Environment.GetEnvironmentVariable("SystemRoot"), "System32", "WindowsPowerShell", "v1.0", "powershell.exe");

    public override string Name => "PowerShell";
    public override ExtensionGroup SupportedExtensions { get; } = new(".ps1");

    protected override IncompleteArguments Arguments { get; } = new("-WindowStyle Hidden -NoLogo -NoProfile -NonInteractive -File \"{0}\"");

    protected override FileInfo Executable { get; } = new(_path);

    public override void ExecuteCode(string code, string scriptName, TimeSpan timeout, Func<string, bool> promptEndTaskOnHung, Func<Exception, FileSystemInfo, FSVerb, bool> promptRetryOnFSError)
        => base.ExecuteCode($"Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process\r\n{code}", scriptName, timeout, promptEndTaskOnHung, promptRetryOnFSError);
}