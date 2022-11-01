using System.Diagnostics;
using System.Globalization;

using Humanizer;

using Microsoft.WindowsAPICodePack.Win32Native;

using Scover.WinClean.DataAccess;

using static System.IO.Path;

namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class Host : IScriptData
{
    /// <summary>Arguments to pass to <see cref="Executable"/> when executing.</summary>
    private readonly IncompleteArguments _arguments;

    /// <summary>The path to the executable of the script host program.</summary>
    private readonly string _executable;

    private Host(string invariantName, string description, string executable, string arguments, params string[] supportedExtensions)
    {
        using ShellFile shellFile = new(executable);
        Name = shellFile.FileDescription;

        InvariantName = invariantName;
        Description = description;
        _executable = executable;
        _arguments = new(arguments);
        SupportedExtensions = new(supportedExtensions);
    }

    public static Host Cmd { get; } = new(nameof(Cmd),
                                                 Resources.Hosts.CmdDescription,
                                                 Join(Environment.SystemDirectory, "cmd.exe"),
                                                 "/d /c \"{0}\"",
                                                 ".cmd", ".bat");

    public static Host PowerShell { get; } = new(nameof(PowerShell),
                                                        Resources.Hosts.PowerShellDescription,
                                                        Join(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"),
                                                        "-ExecutionPolicy Unrestricted -File \"{0}\"",
                                                        ".ps1");

    public static Host Regedit { get; } = new(nameof(Regedit),
                                                         Resources.Hosts.RegeditDescription,
                                                         Join(Environment.SystemDirectory, "..", "regedit.exe"),
                                                         "/s {0}",
                                                         ".reg");

    public string Description { get; }
    public string InvariantName { get; }
    public string Name { get; }

    public ExtensionGroup SupportedExtensions { get; }

    public void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningElseKill, CancellationToken cancellationToken)
    {
        FileInfo tmpScriptFile = CreateTempFile(code, SupportedExtensions.First());

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

        registration.Unregister();
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

    private Process StartHost(FileInfo script)
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