using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

using CommunityToolkit.Mvvm.Input;

using Optional;
using Optional.Unsafe;

using Scover.WinClean.Model;

using Vanara.PInvoke;

using static Vanara.PInvoke.Shell32;

namespace Scover.WinClean;

public static class Extensions
{
    public static bool CanExecute(this IRelayCommand relayCommand) => relayCommand.CanExecute(null);

    public static ISynchronizeInvoke CreateSynchronizationObject(this DispatcherObject dispatcherObject) => new DispatcherSynchronizeInvoke(dispatcherObject.Dispatcher);

    public static void DisposeIfCreated<T>(this Lazy<T> lazy) where T : IDisposable
    {
        if (lazy.IsValueCreated)
        {
            lazy.Value.Dispose();
        }
    }

    public static string FormatToSeconds(this TimeSpan t)
        => Convert.ToInt32(t.TotalSeconds).Seconds().ToString("g");

    public static LocalizedString GetLocalizedString(this XmlDocument doc, string name)
    {
        LocalizedString localizedNodeTexts = new();
        foreach (XmlElement element in doc.GetElementsByTagName(name))
        {
            localizedNodeTexts.SetFromXml(element);
        }
        return localizedNodeTexts;
    }

    /// <summary>Get the process tree for this process.</summary>
    /// <param name="process"></param>
    /// <returns>
    /// The process tree for this process, starting from the deepest descendants to the children.
    /// </returns>
    /// <remarks>The returned collection includes <paramref name="process"/> as the last element.</remarks>
    public static IEnumerable<Process> GetProcessTree(this Process process)
        => new ManagementObjectSearcher($"Select ProcessID From Win32_Process Where ParentProcessID={process.Id}")
        .Get().Cast<ManagementObject>()
        .SelectMany(m => Process.GetProcessById(Convert.ToInt32(m["ProcessID"])).GetProcessTree())
        .Append(process);

    /// <inheritdoc cref="GetSingleChildOrDefault(XmlElement, string)"/>
    /// <returns>The single child element.</returns>
    public static XmlElement GetSingleChild(this XmlElement parent, string name)
        => parent.GetSingleChildOrDefault(name) ?? throw new XmlException($"'{parent.Name}' has no child element named '{name}'.");

    /// <inheritdoc cref="GetSingleChild(XmlElement, string)"/>
    public static XmlElement GetSingleChild(this XmlDocument parent, string name)
        => (parent.DocumentElement ?? throw new XmlException("No root exists in document.")).GetSingleChild(name);

    /// <inheritdoc cref="GetSingleChildOrDefault(XmlElement, string)"/>
    public static XmlElement? GetSingleChildOrDefault(this XmlDocument parent, string name)
        => (parent.DocumentElement ?? throw new XmlException("No root exists in document.")).GetSingleChildOrDefault(name);

    /// <summary>Gets the single child element with the specified name.</summary>
    /// <param name="parent">The parent element.</param>
    /// <param name="name">The tag name of the element to search for.</param>
    /// <returns>The single child element, or <see langword="null"/> if it was not found.</returns>
    /// <exception cref="XmlException">There are multiple elements named <paramref name="name"/>.</exception>
    public static XmlElement? GetSingleChildOrDefault(this XmlElement parent, string name)
    {
        var elements = parent.GetElementsByTagName(name).OfType<XmlElement>();
        return elements.Count() > 1
            ? throw new XmlException($"'{parent.Name}' has {elements.Count()} child elements named '{name}' but only one was expected.")
            : elements.SingleOrDefault();
    }

    /// <returns>The <see cref="XmlNode.InnerText"/> of the node.</returns>
    /// <inheritdoc cref="GetSingleChildTextOrDefault(XmlElement, string)"/>
    public static string GetSingleChildText(this XmlElement parent, string name) => GetSingleChild(parent, name).InnerText;

    /// <inheritdoc cref="GetSingleChildText(XmlElement, string)"/>
    public static string GetSingleChildText(this XmlDocument parent, string name) => GetSingleChild(parent, name).InnerText;

    /// <inheritdoc cref="GetSingleChildTextOrDefault(XmlElement, string)"/>
    public static string? GetSingleChildTextOrDefault(this XmlDocument parent, string name) => GetSingleChildOrDefault(parent, name)?.InnerText ?? default;

    /// <summary>Gets the text of the single child node with the specified name.</summary>
    /// <param name="parent">The parent node.</param>
    /// <param name="name">The tag name of the node to search for.</param>
    /// <returns>
    /// The <see cref="XmlNode.InnerText"/> of the node, or <see langword="null"/> if it was not found.
    /// </returns>
    /// <exception cref="XmlException">There are multiple nodes named <paramref name="name"/>.</exception>
    public static string? GetSingleChildTextOrDefault(this XmlElement parent, string name)
    {
        var elements = parent.GetElementsByTagName(name);
        return elements.Count > 1
            ? throw new XmlException($"'{parent.Name}' has {elements.Count} childs named '{name}' but only one was expected.")
            : elements[0]?.InnerText;
    }

    /// <summary>
    /// Checks if an exception is exogenous and could have been thrown by the filesystem API.
    /// </summary>
    /// <returns>
    /// <para>
    /// <see langword="true"/> if <paramref name="e"/> is of or derived from any of the following types :
    /// </para>
    /// <br><see cref="IOException"/></br><br><see cref="UnauthorizedAccessException"/></br><br><see
    /// cref="SecurityException"/></br>
    /// <para>Otherwise; <see langword="false"/>.</para>
    /// </returns>
    /// <remarks>Note that unrelated methods may throw any of these exceptions.</remarks>
    public static bool IsFileSystemExogenous(this Exception e)
        => e is IOException or UnauthorizedAccessException or SecurityException;

    public static bool IsRunning(this Process process)
    {
        try
        {
            Process.GetProcessById(process.Id).Dispose();
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        {
            return false;
        }
        return true;
    }

    /// <summary>Checks if 2 collections are equivalent and hold the same content.</summary>
    /// <remarks>
    /// Contrarily to <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource},
    /// IEnumerable{TSource})"/>, the order of the elements isn't taken into account.
    /// </remarks>
    public static bool ItemsEqual<T>(this IEnumerable<T> e1, IEnumerable<T> e2) => !e1.Except(e2).Any();

    public static InvalidEnumArgumentException NewInvalidEnumArgumentException<TEnum>(this TEnum value, [CallerArgumentExpression(nameof(value))] string argumentName = "") where TEnum : struct, Enum
        => new(argumentName, Convert.ToInt32(value, CultureInfo.InvariantCulture), typeof(TEnum));

    /// <summary>Asserts that <paramref name="t"/> isn't <see langword="null"/>.</summary>
    /// <remarks>This is a safer replacement for the null-forgiving operator ( <c>!</c>).</remarks>
    /// <returns><paramref name="t"/>, not null.</returns>
    public static T NotNull<T>([NotNull] this T? t, string? message = null)
    {
        Debug.Assert(t is not null, message);
        return t;
    }

    /// <summary>Opens a path with the shell.</summary>
    /// <remarks>
    /// If <paramref name="path"/> is empty or not valid for shell execution, no process will be started.
    /// </remarks>
    public static void Open(this string path) => Process.Start(new ProcessStartInfo(path)
    {
        UseShellExecute = true
    })?.Dispose();

    public static void Resume(this Process process)
    {
        NtResumeProcess(process.Handle);
        Win32Error.ThrowLastError();
    }

    public static void SetFromXml(this LocalizedString str, XmlNode node)
               => str[CultureInfo.GetCultureInfo(node.Attributes?["xml:lang"]?.Value ?? "")] = node.InnerText;

    public static void SetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value) where TKey : notnull
    {
        if (!dic.TryAdd(key, value))
        {
            dic[key] = value;
        }
    }

    public static PropertyGroupDescription SortedBy(this PropertyGroupDescription propertyGroupDescription, string propertyName)
    {
        propertyGroupDescription.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));
        return propertyGroupDescription;
    }

    /// <summary>
    /// Computes the sum of a sequence of time intervals that are obtained by invoking a transform function
    /// on each element of the input sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence of values that are used to calculate a sum.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>The sum of the projected values.</returns>
    public static TimeSpan Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, TimeSpan> selector)
        => source.Aggregate(TimeSpan.Zero, (sumSoFar, nextSource) => sumSoFar + selector(nextSource));

    public static void Suspend(this Process process)
    {
        NtSuspendProcess(process.Handle);
        Win32Error.ThrowLastError();
    }

    public static BitmapSource ToBitmapSource(this SHSTOCKICONID stockIconId, SHGSI flags)
    {
        var info = SHSTOCKICONINFO.Default;
        SHGetStockIconInfo(stockIconId, flags | SHGSI.SHGSI_ICON, ref info).ThrowIfFailed();
        var bitmap = info.hIcon.ToBitmapSource();
        _ = Win32Error.ThrowLastErrorIfFalse(User32.DestroyIcon(info.hIcon));
        return bitmap;
    }

    public static BitmapSource ToBitmapSource(this HICON hIcon)
        => Imaging.CreateBitmapSourceFromHIcon(hIcon.DangerousGetHandle(), Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

    public static IEnumerable<T> WhereSome<T>(this IEnumerable<Option<T>> source) => source.Where(o => o.HasValue).Select(o => o.ValueOrFailure());

    public static IEnumerable<TSource> WithoutNull<TSource>(this IEnumerable<TSource?> source)
                                           => source.Aggregate(Enumerable.Empty<TSource>(), (accumulator, next) => next == null ? accumulator : accumulator.Append(next));

    public static Version WithoutRevision(this Version version) => version.Revision != -1 ? new(version.Major, version.Minor, version.Build) : version;

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern void NtResumeProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern void NtSuspendProcess(IntPtr processHandle);
}