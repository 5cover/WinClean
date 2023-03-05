using System.Diagnostics;

using Scover.WinClean.DataAccess;

using static System.IO.Path;

namespace Scover.WinClean.BusinessLogic;

public sealed record Host : ScriptMetadata
{
    private readonly string _arguments;
    private readonly string _executable;

    /// <param name="executable">
    /// The absolute path to the script host executable. May contain environment variables.
    /// </param>
    /// <param name="arguments">
    /// A formattable argument string passed to the executable. Must have exactly one formattable argument,
    /// <c>{0}</c>. May contain environment variables.
    /// </param>
    /// <param name="extension">The preferred script file extension.</param>
    public Host(LocalizedString name, LocalizedString description, string executable, string arguments, string extension) : base(name, description)
        => (_executable, _arguments, Extension) = (Environment.ExpandEnvironmentVariables(executable), Environment.ExpandEnvironmentVariables(arguments), extension);

    public string Extension { get; }

    /// <summary>Executes code asynchronously.</summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="timeout">
    /// Upon reach, <paramref name="keepRunningElseTerminateHungScript"/> will be called and the script will
    /// be terminated if it returns <see langword="false"/>. Otherwise the timeout will reset.
    /// </param>
    /// <param name="keepRunningElseTerminateHungScript">Callback called when the script is hung.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="TimeoutException">
    /// The script was hung and <paramref name="keepRunningElseTerminateHungScript"/> returned <see
    /// langword="false"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    public async Task Execute(string code, TimeSpan timeout, Func<bool> keepRunningElseTerminateHungScript, CancellationToken cancellationToken)
    {
        string tmpScriptFile = CreateTempFile(code);
        using Process hostProcess = StartHost(tmpScriptFile);
        while (!hostProcess.HasExited)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await hostProcess.WaitForExitAsync(cancellationToken).WithTimeout(timeout);
            }
            catch (TimeoutException)
            {
                if (!keepRunningElseTerminateHungScript())
                {
                    hostProcess.Kill(true);
                    throw;
                }
            }
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
        UseShellExecute = true // necessary for WindowStyle
    }).AssertNotNull();
}