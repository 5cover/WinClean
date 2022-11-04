using System.Diagnostics;
using System.Drawing;
using System.Security;
using System.Text;
using System.Xml;

using Scover.WinClean.BusinessLogic;

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

    /// <summary>Fetches the restore point icon as defined in rstrui.exe.</summary>
    public static Icon GetRestorePointIcon() => Icon.ExtractAssociatedIcon(Path.Join(Environment.SystemDirectory, "rstrui.exe")).AssertNotNull();

    /// <summary>Gets the <see cref="XmlElement.InnerText"/> of the single descendant element with the specified name.</summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="name">The tag name of the element to search for.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">There are multiple elements</exception>
    public static string GetSingleNode(this XmlElement parent, string name) => GetSingleNodeImpl(parent.GetElementsByTagName(name), parent, name);

    /// <inheritdoc cref="GetSingleNode(XmlElement, string)"/>
    public static string GetSingleNode(this XmlDocument parent, string name) => GetSingleNodeImpl(parent.GetElementsByTagName(name), parent, name);

    /// <summary>Checks if an exception is related to the filesystem.</summary>
    /// <returns>
    /// <para><see langword="true"/> if <paramref name="e"/> is of any of the following types :</para>
    /// <br><see cref="IOException"/> (including all derived exceptions)</br><br><see
    /// cref="UnauthorizedAccessException"/></br><br><see cref="NotSupportedException"/></br><br><see cref="SecurityException"/></br>
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
        if (!string.IsNullOrWhiteSpace(path))
        {
            using Process? process = Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }
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

    public static void SetFromXml(this LocalizedString str, XmlNode node)
                                => str.Set(new(node.Attributes?["xml:lang"]?.Value ?? string.Empty), node.InnerText);

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

    public static Stream ToStream(this string value) => new MemoryStream(Encoding.UTF8.GetBytes(value ?? string.Empty));

    private static string GetSingleNodeImpl(XmlNodeList nodes, XmlNode parent, string name) => nodes.Count > 1
                                        ? throw new ArgumentException($"'{parent.Name}' has '{nodes.Count}' childs named '{name}' but only one was expected.")
            : nodes[0]?.InnerText ?? throw new ArgumentException($"'{parent.Name}' has no child named '{name}'.");
}