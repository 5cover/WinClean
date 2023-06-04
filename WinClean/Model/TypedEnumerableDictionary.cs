using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Scover.WinClean.Model;

public class TypedEnumerableDictionary : IReadOnlyDictionary<Type, IEnumerable>
{
    private readonly Dictionary<Type, IEnumerable> _dic = new();

    public IEnumerable<Type> Keys => _dic.Keys;
    public IEnumerable<IEnumerable> Values => _dic.Values;
    int IReadOnlyCollection<KeyValuePair<Type, IEnumerable>>.Count => _dic.Count;
    public IEnumerable this[Type key] => _dic[key];

    public void Add<TKey>(IEnumerable<TKey> value) => _dic.Add(typeof(TKey), value);

    public bool ContainsKey<TKey>() => ContainsKey(typeof(TKey));

    public bool ContainsKey(Type key) => _dic.ContainsKey(key);

    public IEnumerable<TKey> Get<TKey>() => _dic[typeof(TKey)].Cast<TKey>();

    public bool TryGetValue(Type key, [MaybeNullWhen(false)] out IEnumerable value) => _dic.TryGetValue(key, out value);

    public bool TryGetValue<TKey>([MaybeNullWhen(false)] out IEnumerable<TKey> value)
    {
        bool result = TryGetValue(typeof(TKey), out IEnumerable? nonGenericValue);
        value = (IEnumerable<TKey>?)nonGenericValue;
        return result;
    }

    IEnumerator<KeyValuePair<Type, IEnumerable>> IEnumerable<KeyValuePair<Type, IEnumerable>>.GetEnumerator() => _dic.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dic.GetEnumerator();
}
