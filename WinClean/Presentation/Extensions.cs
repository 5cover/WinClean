using System.ComponentModel;
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

    public static IEnumerable<KeyValuePair<string, string>> ParseMockDictionary(this string str, string separator)
    {
        if (str == "")
        {
            yield break;
        }
        var keysAndValues = str.Split(separator);
        if (keysAndValues.Length % 2 == 1)
        {
            throw new ArgumentException("Not every key matches with a value", nameof(str));
        }
        for (int i = 0; i < keysAndValues.Length; i += 2)
        {
            yield return new(keysAndValues[i], keysAndValues[i + 1]);
        }
    }

    public static void ScheduleDisposeOnExit(this IDisposable disposable) => Application.Current.Exit += (sender, args) => disposable.Dispose();

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

    public static string ToMockStringDic<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, string separator, Func<TKey, string>? keyFormatter = null, Func<TValue, string>? valueFormatter = null)
    {
        return separator == ""
            ? throw new ArgumentException("Separator is empty", nameof(separator))
            : new StringBuilder().AppendJoin(separator, keyValuePairs.SelectMany(kv => Flatten(Format(kv.Key, keyFormatter), Format(kv.Value, valueFormatter)))).ToString();
        string? Format<T>(T t, Func<T, string>? formatter)
        {
            var tstr = formatter?.Invoke(t) ?? t?.ToString();
            return tstr?.Contains(separator) ?? false
                ? throw new ArgumentException("One of formatted keys or values contain the separator", nameof(keyValuePairs))
                : tstr;
        }
        static IEnumerable<string?> Flatten(string? s1, string? s2)
        {
            yield return s1;
            yield return s2;
        }
    }
}