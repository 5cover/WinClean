using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Win32Native.Shell;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Scover.WinClean.Operational;

/// <summary>Provides a set of extension methods that fulfill a relatively generic role.</summary>
public static class Helpers
{
    [return: NotNull]
    public static T AssertNotNull<T>(this T? t, string message) where T : class
    {
        Debug.Assert(t is not null, message);
        return t;
    }

    /// <summary>Checks if an exception is of a type that .NET Core's filesystem methods may throw.</summary>
    /// <returns>
    /// <para><see langword="true"/> if <paramref name="e"/> is of any of the following types :</para>
    /// <br><see cref="IOException"/> (including all derived exceptions)</br><br><see
    /// cref="UnauthorizedAccessException"/></br><br><see cref="NotSupportedException"/></br><br><see cref="System.Security.SecurityException"/></br>
    /// <para>Otherwise; <see langword="false"/>.</para>
    /// </returns>
    /// <remarks>Note that unrelated methods may throw any of these exceptions.</remarks>
    public static bool FileSystem(this Exception e)
        => e is IOException or UnauthorizedAccessException or NotSupportedException or System.Security.SecurityException;

    /// <summary>Compares names of filesystem elements such as paths, names or directories or files.</summary>
    /// <remarks>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/> comparison because in NTFS, file and directory names are case insensitive.
    /// </remarks>
    public static bool FileSystemEquals(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

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

    public static string? GetDirectoryNameOnly(this string? s)
            => Path.GetFileName(Path.GetDirectoryName(s));

    /// <summary>Creates a file extension filter for an <see cref="OpenFileDialog"/> control.</summary>
    /// <param name="ofd">The <see cref="OpenFileDialog"/> control to make a filter for.</param>
    /// <param name="exts">The extension to put into the filter.</param>
    public static void MakeFilter(this OpenFileDialog ofd, IEnumerable<ExtensionGroup> exts)
        => ofd.Filter = new StringBuilder().AppendJoin('|', exts.SelectMany(group => new string[]
                                                                              {
                                                                                  $"{group.GetName(0)} ({string.Join(';', group.Select(ext => $"*{ext}"))})",
                                                                                  string.Join(';', group.Select(ext => $"*{ext}"))
                                                                              })).ToString();

    /// <inheritdoc cref="MakeFilter(OpenFileDialog, IEnumerable{ExtensionGroup})"/>
    public static void MakeFilter(this OpenFileDialog ofd, params ExtensionGroup[] exts) => ofd.MakeFilter((IEnumerable<ExtensionGroup>)exts);

    /// <summary>Opens an URL in the system's default browser</summary>
    /// <param name="url">The URL to open.</param>
    public static void OpenUrl(Uri url) => Process.Start(new ProcessStartInfo(url.AbsoluteUri)
    {
        UseShellExecute = true
    })?.Dispose();

    public static Icon ToIcon(this StockIconIdentifier sii)
    {
        using StockIcon stockIcon = new(sii);
        // Using stockIcon.Icon causes ComException (invalid cursor handle) when the icon is displayed. Using stockIcon.Bitmap
        // causes the icon to display incorrectly (low quality and black transparency). We can assume the implementation of
        // StockIcon in the Window API Code Pack is not compatible with the current Icon class.
        return Icon.FromHandle(stockIcon.Icon.ToBitmap().GetHicon());
    }
}