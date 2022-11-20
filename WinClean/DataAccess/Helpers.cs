using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Scover.WinClean.DataAccess;

/// <summary>Provides a set of extension methods that fulfill a relatively generic role.</summary>
public static class Helpers
{
    private static char[] _invalidFileNameChars => Path.GetInvalidFileNameChars();

    /// <summary>Asserts that <paramref name="t"/> isn't <see langword="null"/>.</summary>
    /// <returns><paramref name="t"/>, not null.</returns>
    public static T AssertNotNull<T>([NotNull] this T? t)
    {
        Debug.Assert(t is not null, $"{nameof(t)} is null.");
        return t;
    }

    public static bool EqualsContent<TKey, TValue>(this IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2)
            => d1.Count == d2.Count && !d1.Except(d2).Any();

    public static LocalizedString GetLocalizedString(this XmlDocument doc, string name)
    {
        LocalizedString localizedNodeTexts = new();
        foreach (XmlElement element in doc.GetElementsByTagName(name))
        {
            localizedNodeTexts.SetFromXml(element);
        }
        return localizedNodeTexts;
    }

    /// <summary>Gets the restore point icon stored in rstrui.exe.</summary>
    /// <returns>
    /// The restore point icon, or <see langword="null"/> if the icon was not found (rstrui.exe may be missing or damaged).
    /// </returns>
    public static Icon? GetRestorePointIcon() => Icon.ExtractAssociatedIcon(Path.Join(Environment.SystemDirectory, "rstrui.exe"));

    /// <summary>Gets the single child element with the specified name.</summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="name">The tag name of the element to search for.</param>
    /// <returns>The <see cref="XmlNode.InnerText"/> of the single child node.</returns>
    /// <exception cref="XmlException">There are no or multiple elements named <paramref name="name"/>.</exception>
    public static string GetSingleChild(this XmlElement parent, string name)
    {
        var elements = parent.GetElementsByTagName(name);
        return elements.Count > 1
            ? throw new XmlException($"'{parent.Name}' has {elements.Count} childs named '{name}' but only one was expected.")
            : elements[0]?.InnerText ?? throw new XmlException($"'{parent.Name}' has no child named '{name}'.");
    }

    /// <inheritdoc cref="GetSingleChild(XmlElement, string)"/>
    public static string GetSingleChild(this XmlDocument parent, string name)
        => GetSingleChild(parent.DocumentElement ?? throw new XmlException("No root exists in document."), name);

    /// <summary>Checks if an exception could have been thrown by the filesystem API.</summary>
    /// <returns>
    /// <para><see langword="true"/> if <paramref name="e"/> is of or derived from any of the following types :</para>
    /// <br><see cref="IOException"/></br><br><see cref="UnauthorizedAccessException"/></br><br><see cref="SecurityException"/></br>
    /// <para>Otherwise; <see langword="false"/>.</para>
    /// </returns>
    /// <remarks>Note that unrelated methods may throw any of these exceptions.</remarks>
    public static bool IsFileSystem(this Exception e)
        => e is IOException or UnauthorizedAccessException or SecurityException;

    /// <summary>Opens a file or an URI with the shell.</summary>
    /// <remarks>
    /// If <paramref name="path"/> is <see langword="null"/>, empty, only whitespace, or not valid for shell execution, no
    /// process will be started.
    /// </remarks>
    public static void Open(string? path)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            using Process? process = Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }
    }

    public static void SetFromXml(this LocalizedString str, XmlNode node)
        => str.Set(new(node.Attributes?["xml:lang"]?.Value ?? ""), node.InnerText);

    public static Process? StartPowerShell(string arguments) => Process.Start(new ProcessStartInfo(Path.Join(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"), arguments)
    {
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden
    });

    /// <summary>
    /// Computes the sum of a sequence of time intervals that are obtained by invoking a transform function on each element of
    /// the input sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values that are used to calculate a sum.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>The sum of the projected values.</returns>
    public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        => source.Aggregate(TimeSpan.Zero, (sumSoFar, nextSource) => sumSoFar + selector(nextSource));

    /// <summary>Creates a valid Windows filename from a string.</summary>
    /// <param name="filename">The filename candidate.</param>
    /// <param name="replaceInvalidCharsWith">What to replace invalid filename chars in <paramref name="filename"/> with.</param>
    /// <returns>
    /// A new <see cref="string"/>, equivalent to <paramref name="filename"/>, but modified to be a valid Windows filename if it
    /// <paramref name="filename"/> wasn't already.
    /// </returns>
    /// <exception cref="ArgumentException"/>
    /// <remarks>The length of the filename is not checked, and the casing is not modified.</remarks>
    public static string ToFilename(this string filename, string replaceInvalidCharsWith = "_")
        => string.IsNullOrWhiteSpace(filename)
             ? throw new ArgumentException("Is null, empty, or whitespace.", nameof(filename))

         : string.IsNullOrEmpty(replaceInvalidCharsWith)
             ? throw new ArgumentException("Is null or empty.", nameof(replaceInvalidCharsWith))

         : filename.All(c => c == '.')
             ? throw new ArgumentException("Consists only of dots", nameof(filename))

         : replaceInvalidCharsWith.All(c => c == '.')
             ? throw new ArgumentException("Consists only of dots.", nameof(replaceInvalidCharsWith))

         : replaceInvalidCharsWith.IndexOfAny(_invalidFileNameChars) != -1
             ? throw new ArgumentException("Contains invalid filename chars.", nameof(replaceInvalidCharsWith))

         : Regex.Replace(filename.Trim(), $"[{Regex.Escape(new(_invalidFileNameChars))}]", replaceInvalidCharsWith,
                         RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static Stream ToStream(this string value) => new MemoryStream(Encoding.UTF8.GetBytes(value));
}