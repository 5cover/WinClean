using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Scover.WinClean.Presentation;

/// <summary>
/// Provides a set of extension methods that fulfill a relatively generic role in the <see
/// cref="Presentation"/> layer.
/// </summary>
public static class Extensions
{
    public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T t)
            {
                yield return t;
            }
            foreach (T childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }

    public static InvalidEnumArgumentException NewInvalidEnumArgumentException<TEnum>(this TEnum value, [CallerArgumentExpression(nameof(value))] string argumentName = "") where TEnum : struct, Enum
        => new(argumentName, Convert.ToInt32(value, CultureInfo.InvariantCulture), typeof(TEnum));

    /// <summary>Opens a file or an URI with the shell.</summary>
    /// <remarks>
    /// If <paramref name="path"/> is <see langword="null"/>, empty, or not valid for shell execution, no
    /// process will be started.
    /// </remarks>
    public static void Open(this string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            })?.Dispose();
        }
    }

    public static IEnumerable<(string? key, string? value)> ParseKeysAndValues(this StringCollection strCollection)
    {
        for (int i = 0; i < strCollection.Count; i += 2)
        {
            yield return new(strCollection[i], strCollection[i + 1]);
        }
    }

    /// <summary>
    /// Replaces the content of a string collection with tuples by alternating between keys and values.
    /// </summary>
    /// <param name="strCollection">The string collection to update.</param>
    /// <param name="keysAndValues">The tuples to add to <paramref name="strCollection"/>.</param>
    /// <remarks>Keys are on even indexes. Values are on odd indexes.</remarks>
    public static void SetContent(this StringCollection strCollection, IEnumerable<(string key, string value)> keysAndValues)
    {
        strCollection.Clear();
        foreach (var (key, value) in keysAndValues)
        {
            _ = strCollection.Add(key);
            _ = strCollection.Add(value);
        }
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
}