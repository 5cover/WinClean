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
    /// The script was hung and <paramref name="keepRunningElseTerminateHungScript"/> returned <see langword="false"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
    /// <returns>
    /// A <see cref="ScriptExecution"/> instance that provides information about the ongoing script execution.
    /// </returns>
    public ScriptExecution Execute(string code)
    {
        string tmpScriptFile = CreateTempFile(code);
        ScriptExecution execution = new(GetStartInfo(tmpScriptFile));
        execution.ProcessExited += (s, e) =>
        {
            try
            {
                File.Delete(tmpScriptFile);
            }
            catch (Exception ex) when (ex.IsFileSystemExogenous())
            {
                // It's a temp file, it's fine not to delete it.
            }
        };
        return execution;
    }

    private string CreateTempFile(string text)
    {
        string tmpFile = Join(GetTempPath(), ChangeExtension(GetRandomFileName(), Extension));
        File.WriteAllText(tmpFile, text);
        return tmpFile;
    }

    private ProcessStartInfo GetStartInfo(string tmpScriptFile) => new(_executable, _arguments.FormatWith(tmpScriptFile))
    {
        CreateNoWindow = true,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
    };
}