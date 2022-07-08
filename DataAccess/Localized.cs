using System.Collections;
using System.Globalization;

namespace Scover.WinClean.DataAccess;

public class Localized<T> : IReadOnlyCollection<KeyValuePair<string, T>>
{
    private readonly Dictionary<string, T> _values = new();

    public int Count => _values.Count;

    public T Get(CultureInfo culture)
    {
        T? localized;
        for (; !_values.TryGetValue(culture.Name, out localized) && culture != culture.Parent; culture = culture.Parent)
        {
        }
        return localized ?? throw new ArgumentException("No value was found for this culture or any of its parents.", nameof(culture));
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => _values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_values).GetEnumerator();

    public void Set(CultureInfo culture, T value) => _values[culture.Name] = value;
}