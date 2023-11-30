using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Scover.WinClean.Model;

/// <summary>
/// Dictionary of <see cref="IEnumerable"/> keyed by <see cref="Type"/> with generic helpers.
/// </summary>
public sealed class TypedEnumerableDictionary : IDictionary<Type, IEnumerable>
{
    private readonly Dictionary<Type, IEnumerable> _dic = new();
    public int Count => _dic.Count;
    public ICollection<Type> Keys => _dic.Keys;
    public ICollection<IEnumerable> Values => _dic.Values;
    bool ICollection<KeyValuePair<Type, IEnumerable>>.IsReadOnly => ((ICollection<KeyValuePair<Type, IEnumerable>>)_dic).IsReadOnly;

    public IEnumerable this[Type key]
    {
        get => _dic[key];
        set => _dic[key] = value;
    }

    public void Add(Type key, IEnumerable value) => _dic.Add(key, value);

    public void Add<TKey>(IEnumerable<TKey> value) => _dic.Add(typeof(TKey), value);

    public void Clear() => _dic.Clear();

    public bool ContainsKey(Type key) => _dic.ContainsKey(key);

    public IEnumerable<TKey> Get<TKey>() => _dic[typeof(TKey)].Cast<TKey>();

    public bool Remove(Type key) => _dic.Remove(key);

    public bool TryGetValue(Type key, [MaybeNullWhen(false)] out IEnumerable value) => _dic.TryGetValue(key, out value);

    void ICollection<KeyValuePair<Type, IEnumerable>>.Add(KeyValuePair<Type, IEnumerable> item) => ((ICollection<KeyValuePair<Type, IEnumerable>>)_dic).Add(item);

    bool ICollection<KeyValuePair<Type, IEnumerable>>.Contains(KeyValuePair<Type, IEnumerable> item) => ((ICollection<KeyValuePair<Type, IEnumerable>>)_dic).Contains(item);

    void ICollection<KeyValuePair<Type, IEnumerable>>.CopyTo(KeyValuePair<Type, IEnumerable>[] array, int arrayIndex) => ((ICollection<KeyValuePair<Type, IEnumerable>>)_dic).CopyTo(array, arrayIndex);

    IEnumerator<KeyValuePair<Type, IEnumerable>> IEnumerable<KeyValuePair<Type, IEnumerable>>.GetEnumerator() => _dic.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dic.GetEnumerator();

    bool ICollection<KeyValuePair<Type, IEnumerable>>.Remove(KeyValuePair<Type, IEnumerable> item) => ((ICollection<KeyValuePair<Type, IEnumerable>>)_dic).Remove(item);
}