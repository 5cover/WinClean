using System.Diagnostics;
using System.Runtime.CompilerServices;

using static System.IO.Path;

namespace Scover.WinClean.Operational;

/// <summary>Represents a program that accepts a file in it's command-line arguments.</summary>
public abstract class ScriptHost
{
    public static readonly IEnumerable<string> HostsLocalizedNames = new List<string>
    {
        new Cmd().LocalizedName,
        new PowerShell().LocalizedName,
        new Regedit().LocalizedName
    };

    /// <summary>User friendly name for the script host.</summary>
    public virtual string LocalizedName
    {
        get
        {
            using ShellFile shellFile = new(Executable);
            return shellFile.FileDescription;
        }
    }

    /// <summary>Culture-independent name for the script host.</summary>
    public abstract string Name { get; }

    /// <summary>Extensions of the scripts the script host program can run, in the order of preference.</summary>
    public abstract ExtensionGroup SupportedExtensions { get; }

    /// <summary>Arguments passed along <see cref="Executable"/> when executing.</summary>
    protected abstract IncompleteArguments Arguments { get; }

    /// <summary>The executable of the script host program.</summary>
    protected abstract FileInfo Executable { get; }

    public override bool Equals(object? obj) => obj is ScriptHost host && Name == host.Name;

    /// <summary>Executes the specified code.</summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="scriptName">The name of the script.</param>
    /// <param name="promptEndTaskOnHung">
    /// Delegate invoked when the script is still running after <paramref name="timeout"/> has elapsed and is probably hung.
    /// </param>
    /// <param name="timeout">How long to wait for the script to end before throwing an <see cref="HungScriptException"/>.</param>
    /// <inheritdoc cref="CreateTempFile(string, string, Func{Exception, FileSystemInfo, FSVerb, bool})"/>
    public virtual void ExecuteCode(string code, string scriptName, TimeSpan timeout, Func<string, bool> promptEndTaskOnHung, Func<Exception, FileSystemInfo, FSVerb, bool> promptRetryOnFSError)
    {
        FileInfo tmpScriptFile = CreateTempFile(code, SupportedExtensions[0], promptRetryOnFSError);

        using Process host = ExecuteHost(tmpScriptFile);

        try
        {
            WaitForExit(host, timeout);
        }
        catch (TimeoutException e)
        {
            if (!promptEndTaskOnHung(code))
            {
                throw new HungScriptException(scriptName, e);
            }
        }

        tmpScriptFile.Delete();
    }

    public override int GetHashCode() => HashCode.Combine(Name);

    public override string ToString() => LocalizedName;

    /// <summary>Creates a temporary file with the specified text.</summary>
    /// <returns>The new temporary file.</returns>
    /// <param name="text">The text to write in the temporary file.</param>
    /// <param name="extension">The extension of the file to create.</param>
    /// <param name="promptRetryOnFSError">Delegate invoked when a filesystem error occurs.</param>
    /// <exception cref="System.Security.SecurityException">
    /// The caller does not have the required permission -and- <paramref name="promptRetryOnFSError"/> returned <see langword="false"/>.
    /// </exception>
    /// <exception cref="IOException">
    /// An I/O error occured. -or- The disk is read-only. -and- <paramref name="promptRetryOnFSError"/> returned <see langword="false"/>.
    /// </exception>
    protected static FileInfo CreateTempFile(string text, string extension, Func<Exception, FileSystemInfo, FSVerb, bool> promptRetryOnFSError)
    {
        FileInfo tmp = new(Join(GetTempPath(), ChangeExtension(GetRandomFileName(), extension)));

        try
        {
            using StreamWriter s = tmp.CreateText();
            {
                s.Write(text);
            }
        }
        catch (Exception e) when (e is System.Security.SecurityException or IOException)
        {
            if (!promptRetryOnFSError.Invoke(e, tmp, FSVerb.Create))
            {
                throw;
            }
        }
        return tmp;
    }

    /// <summary>Waits for the end of the specified process.</summary>
    /// <param name="p">The process which to wait for exit.</param>
    /// <param name="timeout">How long to wait for the process to exit before throwing an exception.</param>
    /// <exception cref="TimeoutException">The process didn't exit afer <paramref name="timeout"/>.</exception>
    protected static void WaitForExit(Process p, TimeSpan timeout)
    {
        if (!p.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
        {
            throw new TimeoutException(timeout);
        }
    }

    /// <summary>Executes the script host program with the specified script.</summary>
    /// <param name="script">The script to execute.</param>
    protected Process ExecuteHost(FileInfo script)
        => Process.Start(new ProcessStartInfo(Executable.FullName, Arguments.Complete(script))
        {
            WindowStyle = ProcessWindowStyle.Hidden,
        })!; // ! : it wont return null

    /// <summary>Formattable executable arguments with a single file path argument.</summary>
    protected class IncompleteArguments
    {
        private readonly string _args;

        /// <param name="args">Formattable string with 1 argument, the path of the script file.</param>
        /// <exception cref="ArgumentException"><paramref name="args"/> does not contain exactly one formattable argument.</exception>
        public IncompleteArguments(string args)
        {
            const int ExpectedFormatItemCount = 1;
            if (FormattableStringFactory.Create(args, string.Empty).ArgumentCount != ExpectedFormatItemCount)
            {
                throw new ArgumentException(Resources.DevException.WrongFormatItemCount.FormatWithInvariant(ExpectedFormatItemCount), nameof(args));
            }
            _args = args;
        }

        /// <summary>Completes the arguments with the specified script file.</summary>
        /// <param name="script">The file to complete the arguments with.</param>
        /// <returns>The completed arguments</returns>
        public string Complete(FileInfo script) => _args.FormatWithInvariant(script);
    }
}