using System.Collections;
using System.Globalization;
using System.Xml;

using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic;

public sealed class LocalizedString : IReadOnlyCollection<KeyValuePair<string, string>>
{
    private readonly Dictionary<string, string> _values = new();

    public int Count => _values.Count;

    /// <summary>Gets the localized string corresponding to the given culture.</summary>
    /// <exception cref="ArgumentException">No string was found for this culture or any of its parents.</exception>
    public string Get(CultureInfo culture)
    {
        string? localized;
        for (; !_values.TryGetValue(culture.Name, out localized) && !Equals(culture, culture.Parent); culture = culture.Parent)
        {
        }
        return localized ?? throw new ArgumentException(DevException.NoStringFoundForThisCulture, nameof(culture));
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, string>>)_values).GetEnumerator();

    /// <summary>Sets the localized string for the specified culture.</summary>
    public void Set(CultureInfo culture, string value)
    {
        if (culture.Equals(AppInfo.NeutralResourcesCulture))
        {
            culture = CultureInfo.InvariantCulture;
        }
        _values[culture.Name] = value;
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_values).GetEnumerator();
}