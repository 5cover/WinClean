using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
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

    public static IEnumerable<KeyValuePair<string, string>> ParseMockDictionary(this string str)
    {
        var lines = str.Split(Environment.NewLine);
        if (lines.Length % 2 == 1)
        {
            throw new ArgumentException("Not every key matches with a value", nameof(str));
        }
        for (int i = 0; i < lines.Length; i += 2)
        {
            yield return new(lines[i], lines[i + 1]);
        }
    }

    public static string ToMockStringDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, Func<TKey, string>? keyFormatter = null, Func<TValue, string>? valueFormatter = null)
    {
        StringBuilder strBuilder = new();
        foreach (var (key, value) in keyValuePairs)
        {
            _ = strBuilder
                .AppendLine(keyFormatter?.Invoke(key) ?? key?.ToString())
                .AppendLine(valueFormatter?.Invoke(value) ?? value?.ToString());
        }
        return strBuilder.ToString()[..^Environment.NewLine.Length];
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