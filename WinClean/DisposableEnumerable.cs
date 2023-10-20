using System.Collections;

namespace Scover.WinClean;

public sealed class DisposableEnumerable<T> : IEnumerable<T>, IDisposable where T : IDisposable
{
    private readonly IEnumerable<T> _items;

    public DisposableEnumerable(IEnumerable<T> items) => _items = items;

    public void Dispose()
    {
        foreach (var item in _items)
        {
            item.Dispose();
        }
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}