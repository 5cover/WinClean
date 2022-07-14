using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources;

using System.Diagnostics;
using System.Runtime.CompilerServices;

using static System.IO.Path;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>Represents a program that accepts a file in it's command-line arguments.</summary>
public class Host : IUserVisible
{
    private Host(string executable)
    {
        using ShellFile shellFile = new(executable);
        Name = shellFile.FileDescription;
        Executable = executable;
    }

    public static Host Cmd { get; } = new(Join(Environment.SystemDirectory, "cmd.exe"))
    {
        Description = Resources.Host.CmdDescription,
        InvariantName = nameof(Cmd),
        SupportedExtensions = new(".cmd", ".bat"),
        Arguments = new("/d /c \"{0}\"")
    };

    public static Host PowerShell { get; } = new(Helpers.GetPowerShellPath())
    {
        Description = Resources.Host.PowerShellDescription,
        InvariantName = nameof(PowerShell),
        SupportedExtensions = new(".ps1"),
        Arguments = new("-File \"{0}\"")
    };

    public static Host Regedit { get; } = new(Join(Environment.SystemDirectory, "..", "regedit.exe"))
    {
        Description = Resources.Host.RegeditDescription,
        InvariantName = nameof(Regedit),
        SupportedExtensions = new(".reg"),
        Arguments = new("/s {0}")
    };

    // ! : Using private initializers, looks better than constructor arguments.

    public string Description { get; private init; } = default!;
    public string InvariantName { get; private init; } = default!;
    public string Name { get; }

    /// <summary>Extensions of the scripts the script host program can run, in the order of preference.</summary>
    public ExtensionGroup SupportedExtensions { get; private init; } = default!;

    /// <summary>Arguments to pass to <see cref="Executable"/> when executing.</summary>
    private IncompleteArguments Arguments { get; init; } = default!;

    /// <summary>The path to the executable of the script host program.</summary>
    private string Executable { get; }

    /// <summary>Executes the specified code.</summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="scriptName">The name of the script to execute.</param>
    /// <param name="timeout">The script timeout.</param>
    /// <param name="keepRunningOrKill">Chooses whether a hung script should be allowed to keep running or be killed.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that aborts the execution by killing the host process when the cancelled. Can be <see
    /// langword="null"/> to disable cancellation.
    /// </param>
    public void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningOrKill, CancellationToken? cancellationToken)
    {
        FileInfo tmpScriptFile = CreateTempFile(code, SupportedExtensions.First());

        using Process host = ExecuteHost(tmpScriptFile);

        // If the operation is cancelled, kill the host process. Then, WaitForExit() will take care of it.
        var registration = cancellationToken?.Register(Kill);

        while (true)
        {
            if (host.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
            {
                registration?.Unregister();
                break;
            }

            if (keepRunningOrKill(scriptName)) continue;
            Kill();
            break;
        }
        tmpScriptFile.Delete();

        void Kill() => host.Kill(true);
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
        => Process.Start(new ProcessStartInfo(Executable, Arguments.Complete(script))
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = true // to use ProcessWindowStyle.Hidden
        }).AssertNotNull();

    private class IncompleteArguments
    {
        private readonly string _args;

        public IncompleteArguments(string args)
        {
            const int expectedFormatItemCount = 1;
            if (FormattableStringFactory.Create(args, string.Empty).ArgumentCount != expectedFormatItemCount)
            {
                throw new ArgumentException(DevException.WrongFormatItemCount.FormatWithInvariant(expectedFormatItemCount), nameof(args));
            }
            _args = args;
        }

        public string Complete(FileInfo script) => _args.FormatWithInvariant(script);
    }
}