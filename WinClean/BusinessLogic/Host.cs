using System.Diagnostics;
using Scover.WinClean.DataAccess;
using static System.IO.Path;

namespace Scover.WinClean.BusinessLogic;

public sealed record Host : ScriptMetadata
{
    private readonly string _arguments;
    private readonly string _executable;

    /// <param name="executable">The absolute path to the script host executable. May contain environment variables.</param>
    /// <param name="arguments">
    /// A formattable argument string passed to the executable. Must have exactly one formattable argument, <c>{0}</c>. May
    /// contain environment variables.
    /// </param>
    /// <param name="extension">The preferred script file extension.</param>
    public Host(LocalizedString name, LocalizedString description, string executable, string arguments, string extension) : base(name, description)
        => (_executable, _arguments, Extension) = (Environment.ExpandEnvironmentVariables(executable), Environment.ExpandEnvironmentVariables(arguments), extension);

    public string Extension { get; }

    /// <summary>Executes code asynchronously.</summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="OperationCanceledException"/>
    public async Task Execute(string code, CancellationToken cancellationToken)
    {
        string tmpScriptFile = CreateTempFile(code);
        using Process hostProcess = StartHost(tmpScriptFile);

        await hostProcess.WaitForExitAsync(cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            hostProcess.Kill(true);
        }

        File.Delete(tmpScriptFile);
    }

    /// <summary>Executes code synchronously.</summary>
    /// <param name="code">The code to execute.</param>
    public void Execute(string code)
    {
        string tmpScriptFile = CreateTempFile(code);
        using Process hostProcess = StartHost(tmpScriptFile);
        hostProcess.WaitForExit();
        File.Delete(tmpScriptFile);
    }

    private string CreateTempFile(string text)
    {
        string tmpFile = Join(GetTempPath(), ChangeExtension(GetRandomFileName(), Extension));
        File.WriteAllText(tmpFile, text);
        return tmpFile;
    }

    private Process StartHost(string script) => Process.Start(new ProcessStartInfo(_executable, _arguments.FormatWith(script))
    {
        WindowStyle = ProcessWindowStyle.Hidden,
        UseShellExecute = true // necessary for ProcessWindowStyle.Hidden
    }).AssertNotNull();
}