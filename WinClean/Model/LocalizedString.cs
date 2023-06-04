using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Scover.WinClean.Model;

/// <summary>A string available in multiple languages.</summary>
/// <remarks>Contrarily to resources, localized strings are dynamic and can be modified at runtime.</remarks>
public sealed class LocalizedString : IDictionary<CultureInfo, string>, IEquatable<LocalizedString?>
{
    private readonly Dictionary<CultureInfo, string> _values;

    public LocalizedString(IDictionary<CultureInfo, string> values) => _values = new(values);

    public LocalizedString() => _values = new();

    public int Count => _values.Count;

    public ICollection<CultureInfo> Keys => _values.Keys;
    public ICollection<string> Values => _values.Values;
    bool ICollection<KeyValuePair<CultureInfo, string>>.IsReadOnly => ((IDictionary<CultureInfo, string>)_values).IsReadOnly;

    /// <summary>Gets the localized string corresponding to the given culture.</summary>
    /// <exception cref="KeyNotFoundException">
    /// No string was found for this culture or any of its parents.
    /// </exception>
    public string this[CultureInfo key]
    {
        get
        {
            string? localized;
            for (CultureInfo culture = key;
                !_values.TryGetValue(culture, out localized) && !culture.Equals(CultureInfo.InvariantCulture); // InvariantCulture is the root of all cultures
                culture = culture.Parent)
            {
            }
            return localized ?? throw new KeyNotFoundException($"No string was found for this culture ('{key}') or any of its parents.");
        }
        set => _values[key] = value;
    }

    public void Clear() => _values.Clear();

    public bool ContainsKey(CultureInfo key) => _values.ContainsKey(key);

    public override bool Equals(object? obj) => Equals(obj as LocalizedString);

    public bool Equals(LocalizedString? other) => other is not null && _values.EqualsContent(other._values);

    public IEnumerator<KeyValuePair<CultureInfo, string>> GetEnumerator() => _values.GetEnumerator();

    public override int GetHashCode() => HashCode.Combine(_values);

    public bool Remove(CultureInfo key) => _values.Remove(key);

    public bool TryGetValue(CultureInfo key, [MaybeNullWhen(false)] out string value) => _values.TryGetValue(key, out value);

    void IDictionary<CultureInfo, string>.Add(CultureInfo key, string value) => _values.Add(key, value);

    void ICollection<KeyValuePair<CultureInfo, string>>.Add(KeyValuePair<CultureInfo, string> item) => ((IDictionary<CultureInfo, string>)_values).Add(item);

    bool ICollection<KeyValuePair<CultureInfo, string>>.Contains(KeyValuePair<CultureInfo, string> item) => ((IDictionary<CultureInfo, string>)_values).Contains(item);

    void ICollection<KeyValuePair<CultureInfo, string>>.CopyTo(KeyValuePair<CultureInfo, string>[] array, int arrayIndex) => ((IDictionary<CultureInfo, string>)_values).CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    bool ICollection<KeyValuePair<CultureInfo, string>>.Remove(KeyValuePair<CultureInfo, string> item) => ((IDictionary<CultureInfo, string>)_values).Remove(item);
}