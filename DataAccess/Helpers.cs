﻿using System.Diagnostics;
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

    /// <summary>Fetches the restore point icon as defined in rstrui.exe</summary>
    public static Icon GetRestorePointIcon()
        => Icon.ExtractAssociatedIcon(Path.Join(Environment.SystemDirectory, "rstrui.exe")).AssertNotNull();

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

    /// <summary>Opens an Explorer window in the specified directory.</summary>
    /// <param name="dirPath">
    /// The directory the new Explorer window will be opened in. Can be <see langword="null"/> or empty to open Explorer to the
    /// default directory (usually "My computer").
    /// </param>
    public static void OpenExplorerToDirectory(string? dirPath)
    {
        using Process? process = Process.Start("explorer", $"/root,{dirPath}");
    }
}