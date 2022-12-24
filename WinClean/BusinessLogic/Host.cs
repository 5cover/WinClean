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

    /// <summary>Executes code.</summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="tieout">The time to wait for the execution to finish until calling <paramref name="onHung"/>.</param>
    /// <param name="onHung">
    /// <param name="onHung">Callback called when <paramref name="timeout"/> has been reached and the execution is unlikely to finish.</param>
    /// </param>
    /// <param name="cancellationToken">A cancellation token that allows cancellation of the execution.</param>
    public void ExecuteCode(string code, TimeSpan timeout, Action onHung, CancellationToken cancellationToken)
    {
        string tmpScriptFile = CreateTempFile(code);
        using Process host = StartHost(tmpScriptFile);
        using var registration = cancellationToken.Register(() => host.Kill(true));

        await hostProcess.WaitForExitAsync(cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            onHung();
        }

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