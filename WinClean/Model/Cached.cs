namespace Scover.WinClean.Model;

/// <summary>
/// A cached, lazily-initialized value that can be retrieved synchronously or asynchronously.
/// </summary>
public sealed class Cached<T>
{
    private readonly Func<T> _createValue;
    private readonly Func<CancellationToken, Task<T>> _createValueAsync;
    private Lazy<T> _value; // Lazy<T> is thread-safe by default.

    /// <param name="createValue">The function that creates the value synchronously.</param>
    /// <param name="createValueAsync">The function that creates the value asynchronously.</param>
    public Cached(Func<T> createValue, Func<CancellationToken, Task<T>> createValueAsync)
        => (_createValue, _createValueAsync, _value) = (createValue, createValueAsync, new(createValue));

    /// <summary>Retrieves the value synchronously.</summary>
    /// <remarks>
    /// WPF: Use <c>IsAsync=True</c> when binding to this property. This property is thread-safe.
    /// </remarks>
    public T Value => _value.Value;

    /// <summary>Retrieves the value asynchronously.</summary>
    /// <remarks>This method is not thread-safe.</remarks>
    public async ValueTask<T> GetValueAsync(CancellationToken cancellationToken)
    {
        if (_value.IsValueCreated)
        {
            return Value;
        }

        T value = await _createValueAsync(cancellationToken);
        _value = new(value);
        return value;
    }

    /// <summary>
    /// Invalidates the cached value if it exists, so the creator methods are called again.
    /// </summary>
    /// <remarks>This method is not thread-safe.</remarks>
    public void InvalidateValue() => _value = new(_createValue);
}