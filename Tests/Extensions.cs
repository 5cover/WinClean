using System.Globalization;
using System.Text;

using Scover.WinClean.Model;

namespace Tests;

public static class Extensions
{
    public static StringBuilder FormatXml(this LocalizedString localizedString, string elementName)
        => localizedString.FormatXml(elementName, FormatLocalized);

    public static StringBuilder FormatXml<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> kvps, string elementName, Func<string, TKey, TValue, string> format)
        => kvps.Aggregate(new StringBuilder(), (sum, kv) => sum.Append(format(elementName, kv.Key, kv.Value)));

    public static Stream ToStream(this string value) => new MemoryStream(Encoding.UTF8.GetBytes(value));

    private static string FormatLocalized(string elementName, CultureInfo lang, string value) => $@"<{elementName} xml:lang=""{lang.Name}"">{value}</{elementName}>";
}