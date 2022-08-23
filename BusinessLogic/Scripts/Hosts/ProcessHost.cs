using System.Diagnostics;
using System.Globalization;

using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources;

using static System.IO.Path;

namespace Scover.WinClean.BusinessLogic.Scripts.Hosts;

public class ProcessHost : IHost
{
    /// <summary>Arguments to pass to <see cref="Executable"/> when executing.</summary>
    private readonly IncompleteArguments _arguments;

    /// <summary>The path to the executable of the script host program.</summary>
    private readonly string _executable;

    private ProcessHost(string invariantName, string description, string executable, string arguments, params string[] supportedExtensions)
    {
        using ShellFile shellFile = new(executable);
        Name = shellFile.FileDescription;

        InvariantName = invariantName;
        Description = description;
        _executable = executable;
        _arguments = new(arguments);
        SupportedExtensions = new(supportedExtensions);
    }

    public static ProcessHost Cmd { get; } = new(nameof(Cmd),
                                                 Host.CmdDescription,
                                                 Join(Environment.SystemDirectory, "cmd.exe"),
                                                 "/d /c \"{0}\"",
                                                 ".cmd", ".bat");

    public static ProcessHost Regedit { get; } = new(nameof(Regedit),
                                                     Host.RegeditDescription,
                                                     Join(Environment.SystemDirectory, "..", "regedit.exe"),
                                                     "/s {0}",
                                                     ".reg");

    public string Description { get; }
    public string InvariantName { get; }
    public string Name { get; }

    public ExtensionGroup SupportedExtensions { get; }

    public void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningOrKill, CancellationToken cancellationToken)
    {
        ManualResetEventSlim exited = new();

        FileInfo tmpScriptFile = CreateTempFile(code, SupportedExtensions.First());

        using Process host = ExecuteHost(tmpScriptFile);
        host.Exited += (_, _) => exited.Set();

        try
        {
            while (!exited.Wait(timeout, cancellationToken))
            {
                if (!keepRunningOrKill(scriptName))
                {
                    host.Kill(true);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            host.Kill(true);
        }

        tmpScriptFile.Delete();
    }

    private static FileInfo CreateTempFile(string text, string extension)
    {
        FileInfo tmp = new(Join(GetTempPath(), ChangeExtension(GetRandomFileName(), extension)));

        using StreamWriter s = tmp.CreateText();
        {
            s.Write(text);
        }
        return tmp;
    }

    private Process ExecuteHost(FileInfo script)
        => Process.Start(new ProcessStartInfo(_executable, _arguments.Complete(script))
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true // to use ProcessWindowStyle.Hidden
        }).AssertNotNull();

    private class IncompleteArguments
    {
        private readonly string _args;

        /// <summary>Initializes a new <see cref="IncompleteArguments"/> object.</summary>
        /// <param name="args">
        /// The formattable strings corresponding to the arguments. Must have exactly one formattable argument ( <c>{0}</c>).
        /// </param>
        public IncompleteArguments(string args) => _args = args;

        public string Complete(FileInfo script) => _args.FormatWith(CultureInfo.InvariantCulture, script.FullName);
    }
}