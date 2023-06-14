using System.Diagnostics;

namespace Scover.WinClean.Model;

/// <summary>A lazily-initialized value that can be retrieved synchronously or asynchronously.</summary>
public sealed class BiLazy<T>
{
    private readonly Func<CancellationToken, Task<T>> _createValueAsync;
    private Lazy<T> _value; // Lazy<T> is thread-safe by default.

    /// <param name="createValue">The function that creates the value synchronously.</param>
    /// <param name="createValueAsync">The function that creates the value asynchronously.</param>
    public BiLazy(Func<T> createValue, Func<CancellationToken, Task<T>> createValueAsync)
        => (_createValueAsync, _value) = (createValueAsync, new(createValue));

    /// <summary>Retrieves the value synchronously.</summary>
    /// <remarks>
    /// WPF: Use <c>IsAsync=True</c> when binding to this property. This property is thread-safe.
    /// </remarks>
    public T Value
    {
        get
        {
            if (_value.IsValueCreated)
            {
                Debug.WriteLine("Value: returning cached value");
            }
            else
            {
                Debug.WriteLine("Value: creating value");
            }
            return _value.Value;
        }
    }

    /// <summary>Retrieves the value asynchronously.</summary>
    /// <remarks>This method is not thread-safe.</remarks>
    public async ValueTask<T> GetValueAsync(CancellationToken cancellationToken)
    {
        if (_value.IsValueCreated)
        {
            Debug.WriteLine("GetValueAsync: returning cached value");
            return Value;
        }
        Debug.WriteLine("GetValueAsync: creating value");
        T value = await _createValueAsync(cancellationToken);
        _value = new(value);
        return value;
    }
}