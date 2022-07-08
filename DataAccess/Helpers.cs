using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace Scover.WinClean.DataAccess;

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

    /// <summary>Opens an URL in the system's default browser</summary>
    /// <param name="url">The URL to open.</param>
    public static void OpenUrl(Uri url) => Process.Start(new ProcessStartInfo(url.AbsoluteUri)
    {
        UseShellExecute = true
    })?.Dispose();

    public static void SetFromXml(this Localized<string> localizedString, XmlNode node)
        => localizedString.Set(new(node.Attributes?["xml:lang"]?.Value ?? string.Empty), node.InnerText);
}