using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Security;

namespace Scover.WinClean.DataAccess;

/// <summary>Provides a set of extension methods that fulfill a relatively generic role.</summary>
public static class Helpers
{
    /// <summary>Asserts that <paramref name="t"/> isn't <see langword="null"/>.</summary>
    /// <returns><paramref name="t"/>, non-nullable.</returns>
    public static T AssertNotNull<T>(this T? t)
    {
        Debug.Assert(t is not null, $"{nameof(t)} is null.");
        return t;
    }

    /// <summary>Executes a single PowerShell command and waits until the command has finished executing.</summary>
    /// <param name="command">The command to execute.</param>
    public static void ExecutePowerShellCommand(string command)
    {
        using Process? powerShell = Process.Start(new ProcessStartInfo(GetPowerShellPath(), $"-Command {command}")
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
        powerShell?.WaitForExit();
    }

    /// <summary>Replaces the format items in this instance with the string representations of corresponding objects.</summary>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>
    /// A copy of this instance in which the format items have been replaced by the string representation of the corresponding objects.
    /// </returns>
    /// <remarks>Uses <see cref="CultureInfo.CurrentCulture"/> as formatting information.</remarks>
    public static string FormatWith(this string format, params object?[] args) => string.Format(CultureInfo.CurrentCulture, format, args);

    /// <remarks>Uses <see cref="CultureInfo.InvariantCulture"/> formatting information.</remarks>
    /// <inheritdoc cref="FormatWith(string, object[])"/>
    public static string FormatWithInvariant(this string format, params object?[] args) => string.Format(CultureInfo.InvariantCulture, format, args);

    /// <summary>Returns the path of the PowerShell executable on this system.</summary>
    public static string GetPowerShellPath() => Path.Join(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");

    /// <summary>Fetches the restore point icon as defined in rstrui.exe</summary>
    public static Icon GetRestorePointIcon() => Icon.ExtractAssociatedIcon(Path.Join(Environment.SystemDirectory, "rstrui.exe")).AssertNotNull();

    /// <summary>Checks if an exception is related to the filesystem.</summary>
    /// <returns>
    /// <para><see langword="true"/> if <paramref name="e"/> is of any of the following types :</para>
    /// <br><see cref="IOException"/> (including all derived exceptions)</br><br><see
    /// cref="UnauthorizedAccessException"/></br><br><see cref="NotSupportedException"/></br><br><see cref="System.Security.SecurityException"/></br>
    /// <para>Otherwise; <see langword="false"/>.</para>
    /// </returns>
    /// <remarks>Note that unrelated methods may throw any of these exceptions.</remarks>
    public static bool IsFileSystem(this Exception e)
        => e is IOException or UnauthorizedAccessException or NotSupportedException or SecurityException;

    /// <summary>Opens a file or an URI with the shell.</summary>
    /// <remarks>
    /// If <paramref name="path"/> is <see langword="null"/>, empty, only whitespace, or not valid for shell execution, no
    /// process will be started.
    /// </remarks>
    public static void Open(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        using Process? process = Process.Start(new ProcessStartInfo(path)
        {
            UseShellExecute = true
        });
    }
}