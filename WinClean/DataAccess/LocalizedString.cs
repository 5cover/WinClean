using System.Collections;
using System.Globalization;

namespace Scover.WinClean.DataAccess;

/// <summary>A string available in multiple languages.</summary>
public sealed class LocalizedString : IEnumerable<KeyValuePair<string, string>>, IEquatable<LocalizedString?>
{
    private readonly Dictionary<string, string> _values = new();

    public override bool Equals(object? obj) => Equals(obj as LocalizedString);

    public bool Equals(LocalizedString? other) => other is not null && _values.EqualsContent(other._values);

    /// <summary>Gets the localized string corresponding to the given culture.</summary>
    /// <exception cref="KeyNotFoundException">
    /// No string was found for this culture or any of its parents.
    /// </exception>
    public string Get(CultureInfo culture)
    {
        string? localized;
        for (; !_values.TryGetValue(culture.Name, out localized) && !Equals(culture, culture.Parent); culture = culture.Parent)
        {
        }
        return localized ?? throw new KeyNotFoundException($"No string was found for this culture ('{culture}') or any of its parents.");
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)_values).GetEnumerator();

    public override int GetHashCode() => HashCode.Combine(_values);

    /// <summary>Sets the localized string for the specified culture.</summary>
    public void Set(CultureInfo culture, string value) => _values[culture.Name] = value;

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
}