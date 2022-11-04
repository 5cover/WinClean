using System.Diagnostics;

using Humanizer;

using Scover.WinClean.DataAccess;

using static System.IO.Path;

namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class Host : ScriptMetadata
{
    /// <summary>Arguments to pass to <see cref="Executable"/> when executing.</summary>
    private readonly string _arguments;

    /// <summary>The path to the executable of the script host program.</summary>
    private readonly string _executable;

    /// <summary>Initializes a new <see cref="ScriptMetadata"/> object.</summary>
    /// <inheritdoc cref="ScriptMetadata(LocalizedString, LocalizedString)" path="/param"/>
    /// <param name="executable">The absolute path to the script host executable. May contain environment variables.</param>
    /// <param name="arguments">
    /// A formattable argument string passed to the executable. Must have exactly one formattable argument ( <c>{0}</c>). May
    /// contain environment variables.
    /// </param>
    /// <param name="extension">The preferred script file extension.</param>
    public Host(LocalizedString name, LocalizedString description, string executable, string arguments, string extension) : base(name, description)
        => (_executable, _arguments, Extension) = (Environment.ExpandEnvironmentVariables(executable), Environment.ExpandEnvironmentVariables(arguments), extension);

    public string Extension { get; }

    public void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningElseKill, CancellationToken cancellationToken)
    {
        FileInfo tmpScriptFile = CreateTempFile(code);

        using Process host = StartHost(tmpScriptFile);
        using var registration = cancellationToken.Register(() => host.Kill(true));

        while (!host.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
        {
            if (!keepRunningElseKill(scriptName))
            {
                host.Kill(true);
                break;
            }
        }

        _ = registration.Unregister();
        tmpScriptFile.Delete();
    }

    private FileInfo CreateTempFile(string text)
    {
        FileInfo tmpFile = new(Join(GetTempPath(), ChangeExtension(GetRandomFileName(), Extension)));

        using StreamWriter s = tmpFile.CreateText();
        {
            s.Write(text);
        }
        return tmpFile;
    }

    private Process StartHost(FileInfo script)
        => Process.Start(new ProcessStartInfo(_executable, _arguments.FormatWith(script.FullName))
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true // necessary for ProcessWindowStyle.Hidden
        }).AssertNotNull();
}