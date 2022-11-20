using System.Diagnostics;

using Humanizer;

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
    /// <param name="timeout">The time to wait for the execution to finish until calling <paramref name="onHung"/>.</param>
    /// <param name="keepRunningElseTerminate">
    /// <inheritdoc cref="HungScriptCallback" path="/summary"/> Returns <inheritdoc cref="HungScriptCallback" path="/returns"/>
    /// </param>
    /// <param name="cancellationToken">A cancellation token that allows cancellation of the execution.</param>
    public void ExecuteCode(string code, TimeSpan timeout, Func<bool> keepRunningElseTerminate, CancellationToken cancellationToken)
    {
        string tmpScriptFile = CreateTempFile(code);
        using Process host = StartHost(tmpScriptFile);
        using var registration = cancellationToken.Register(Terminate);

        while (!host.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
        {
            if (!keepRunningElseTerminate())
            {
                Terminate();
            }
        }

        _ = registration.Unregister();
        File.Delete(tmpScriptFile);

        void Terminate() => host.Kill(true);
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