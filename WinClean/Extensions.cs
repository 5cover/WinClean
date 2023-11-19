using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;

using CommandLine;

using CommunityToolkit.Mvvm.Input;

using Humanizer.Localisation;

using Optional;
using Optional.Unsafe;

using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;

using Vanara.PInvoke;

using static Vanara.PInvoke.Shell32;

namespace Scover.WinClean;

public static class Extensions
{
    private static readonly Stack<object> handlingCollectionsSendUpdatesTo = new();

    public static bool CanExecute(this IRelayCommand relayCommand) => relayCommand.CanExecute(null);

    public static ISynchronizeInvoke CreateSynchronizationObject(this DispatcherObject dispatcherObject) => new DispatcherSynchronizeInvoke(dispatcherObject.Dispatcher);

    public static void DisposeIfCreated<T>(this Lazy<T> lazy) where T : IDisposable
    {
        if (lazy.IsValueCreated)
        {
            lazy.Value.Dispose();
        }
    }

    public static string FormatMessage(this string message, Dictionary<string, object?> args)
                      => ServiceProvider.Get<IMessageFormatter>().Format(message, args);

    public static LocalizedString GetLocalizedString(this XmlDocument doc, string name)
    {
        LocalizedString localizedNodeTexts = new();
        foreach (XmlElement element in doc.GetElementsByTagName(name))
        {
            localizedNodeTexts.SetFromXml(element);
        }
        return localizedNodeTexts;
    }

    /// <summary>Gets the process tree for this process.</summary>
    /// <param name="process"></param>
    /// <returns>
    /// The process tree for this process, starting from the deepest descendants to the children.
    /// </returns>
    /// <remarks>The returned collection includes <paramref name="process"/> as the last element.</remarks>
    /// <inheritdoc cref="Process.Id" path="/exception"/>
    public static IEnumerable<Process> GetProcessTree(this Process process)
        => new ManagementObjectSearcher($"Select ProcessID From Win32_Process Where ParentProcessID={process.Id}")
        .Get().Cast<ManagementObject>()
        .SelectMany(m => Process.GetProcessById(Convert.ToInt32(m["ProcessID"])).GetProcessTree())
        .Append(process);

    /// <inheritdoc cref="GetSingleChildOrDefault(XmlElement, string)"/>
    /// <returns>The single child element.</returns>
    public static XmlElement GetSingleChild(this XmlElement parent, string name)
        => parent.GetSingleChildOrDefault(name) ?? throw new XmlException(ExceptionMessages.ElementHasNoNamedChild.FormatWith(parent.Name, name));

    /// <inheritdoc cref="GetSingleChild(XmlElement, string)"/>
    public static XmlElement GetSingleChild(this XmlDocument parent, string name)
        => (parent.DocumentElement ?? throw new XmlException(ExceptionMessages.NoRootExists)).GetSingleChild(name);

    /// <inheritdoc cref="GetSingleChildOrDefault(XmlElement, string)"/>
    public static XmlElement? GetSingleChildOrDefault(this XmlDocument parent, string name)
        => (parent.DocumentElement ?? throw new XmlException(ExceptionMessages.NoRootExists)).GetSingleChildOrDefault(name);

    /// <summary>Gets the single child element with the specified name.</summary>
    /// <param name="parent">The parent element.</param>
    /// <param name="name">The tag name of the element to search for.</param>
    /// <returns>The single child element, or <see langword="null"/> if it was not found.</returns>
    /// <exception cref="XmlException">There are multiple elements named <paramref name="name"/>.</exception>
    public static XmlElement? GetSingleChildOrDefault(this XmlElement parent, string name)
    {
        var elements = parent.GetElementsByTagName(name).OfType<XmlElement>();
        return elements.Count() > 1
            ? throw new XmlException(ExceptionMessages.ElementHasMultipleNamedChilds.FormatWith(parent.Name, name, elements.Count()))
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
            ? throw new XmlException(ExceptionMessages.ElementHasMultipleNamedChilds.FormatWith(parent.Name, name, elements.Count))
            : elements[0]?.InnerText;
    }

    public static T? GetVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
    {
        if (depObj is null)
        {
            return null;
        }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); ++i)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            var result = (child as T) ?? GetVisualChild<T>(child);
            if (result is not null)
            {
                return result;
            }
        }
        return null;
    }

    public static string HumanizeToMilliseconds(this TimeSpan t, CultureInfo? culture = null)
        => t.Humanize(culture: culture, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Hour);

    public static string HumanizeToSeconds(this TimeSpan t, CultureInfo? culture = null)
        => t.Humanize(culture: culture, precision: 2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Year);

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

    /// <inheritdoc cref="Process.Kill(bool)" path="/summary"/>
    public static void KillTree(this Process process)
    {
        if (process.IsRunning())
        {
            // Not using process.Kill(true) because: It throws Win32Exceptions for 'Access denied' errors
            // internally. They are handled by the Process class, so it's not a problem, but they're still
            // visible by the debugger. This is because this method enumerates all system processes to build
            // the process tree, but doesn't have permission to open all of them.
            foreach (var p in process.GetProcessTree())
            {
                p.Kill();
            }
        }
    }

    public static InvalidEnumArgumentException NewInvalidEnumArgumentException<TEnum>(this TEnum value, [CallerArgumentExpression(nameof(value))] string argumentName = "") where TEnum : struct, Enum
           => new(argumentName, Convert.ToInt32(value, CultureInfo.InvariantCulture), typeof(TEnum));

    /// <summary>Asserts that <paramref name="t"/> isn't <see langword="null"/>.</summary>
    /// <remarks>This is a safer replacement for the null-forgiving operator (<c>!</c>).</remarks>
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

    /// <summary>
    /// Performs a file system operation and wraps any filesystem exception thrown in a <see cref="FileSystemException"/>.
    /// </summary>
    /// <param name="fsObject">The filesystem object operated on represented a string</param>
    /// <param name="fsOperation">The opeation to perform</param>
    /// <param name="verb">The verb corrresponding to the type of operation</param>
    /// <param name="message">The message to give to the <see cref="FileSystemException"/>.</param>
    /// <exception cref="FileSystemException">A filesystem exception occured.</exception>
    public static void PerformFileSystemOperation(this string fsObject, Action<string> fsOperation, FSVerb verb, string? message = null)
    {
        try
        {
            fsOperation(fsObject);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, verb, fsObject, message);
        }
    }

    /// <inheritdoc cref="Extensions.PerformFileSystemOperation(string, Action{string}, FSVerb, string?)"/>
    public static T PerformFileSystemOperation<T>(this string fsObject, Func<string, T> fsOperation, FSVerb verb, string? message = null)
    {
        try
        {
            return fsOperation(fsObject);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, verb, fsObject, message);
        }
    }

    public static void Resume(this Process process)
    {
        NtResumeProcess(process.Handle);
        Win32Error.ThrowLastError();
    }

    public static void SendUpdatesTo<TSourceItem, TTargetItem>(this ObservableCollection<TSourceItem> source, ICollection<TTargetItem> collection, Func<TSourceItem, TTargetItem>? converter = null, Func<TSourceItem, bool>? filter = null)
           => SendUpdatesTo<ObservableCollection<TSourceItem>, TSourceItem, TTargetItem>(source, collection, converter, filter);

    public static void SendUpdatesTo<TSourceCollection, TSourceItem, TTargetItem>(this TSourceCollection source, ICollection<TTargetItem> collection, Func<TSourceItem, TTargetItem>? converter = null, Func<TSourceItem, bool>? filter = null) where TSourceCollection : IEnumerable<TSourceItem>, INotifyCollectionChanged
           => source.CollectionChanged += (_, e) =>
           {
               if (handlingCollectionsSendUpdatesTo.TryPeek(out var top) && top == collection)
               {
                   return;
               }

               converter ??= i => i.Cast<TTargetItem>();
               filter ??= i => true;

               handlingCollectionsSendUpdatesTo.Push(source);

               // When the action is Reset, no other properties of the event arguments are valid.
               // This means that we cannot use e.OldItems.
               if (e.Action is NotifyCollectionChangedAction.Reset)
               {
                   collection.Clear();
                   foreach (var item in source.Where(filter).Select(converter))
                   {
                       collection.Add(item);
                   }
               }
               else
               {
                   if (e.NewItems is not null)
                   {
                       foreach (var item in e.NewItems.Cast<TSourceItem>().Where(filter).Select(converter))
                       {
                           collection.Add(item);
                       }
                   }
                   if (e.OldItems is not null)
                   {
                       foreach (var item in e.OldItems.Cast<TSourceItem>().Where(filter).Select(converter))
                       {
                           _ = collection.Remove(item);
                       }
                   }
               }

               _ = handlingCollectionsSendUpdatesTo.Pop();
           };

    public static void SetFromXml(this LocalizedString str, XmlNode node)
                  => str[CultureInfo.GetCultureInfo(node.Attributes?["xml:lang"]?.Value ?? "")] = node.InnerText;

    public static void SetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value) where TKey : notnull
    {
        if (!dic.TryAdd(key, value))
        {
            dic[key] = value;
        }
    }

    /// <summary>Shows a dialog modally. This is the default for interface consistency.</summary>
    public static ButtonBase? ShowDialog(this Dialog dialog)
    {
        dialog.StartupLocation = WindowLocation.CenterParent;
        return dialog.Show(ParentWindow.Active);
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

    public static DisposableEnumerable<T> ToDisposableEnumerable<T>(this IEnumerable<T> disposables) where T : IDisposable
                                                                                                                   => new(disposables);

    public static IEnumerable<T> WhereSome<T>(this IEnumerable<Option<T>> source) => source.Where(o => o.HasValue).Select(o => o.ValueOrFailure());

    public static IEnumerable<TSource> WithoutNull<TSource>(this IEnumerable<TSource?> source)
        => source.Aggregate(Enumerable.Empty<TSource>(), (accumulator, next) => next == null ? accumulator : accumulator.Append(next));

    public static Version WithoutRevision(this Version version) => version.Revision != -1 ? new(version.Major, version.Minor, version.Build) : version;

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern void NtResumeProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern void NtSuspendProcess(IntPtr processHandle);
}