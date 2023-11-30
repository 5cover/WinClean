using System.Collections;
using System.Diagnostics;
using System.Globalization;

using Scover.WinClean.Resources;

namespace Scover.WinClean.Model;

/// <summary>A string available in multiple languages.</summary>
/// <remarks>Contrarily to resources, localized strings are dynamic and can be modified at runtime.</remarks>
[DebuggerDisplay($"{{{nameof(_values)}}}")]
public sealed class LocalizedString : IEnumerable<KeyValuePair<CultureInfo, string>>
{
    private readonly Dictionary<CultureInfo, string> _values = new();

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
            return localized ?? throw new KeyNotFoundException(ExceptionMessages.NoStringFoundForCultureTree.FormatWith(key));
        }
        set => _values[key] = value;
    }

    public IEnumerator<KeyValuePair<CultureInfo, string>> GetEnumerator() => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
}