using System.ComponentModel;

namespace Scover.WinClean.ViewModel;

public sealed class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
{
    public ItemPropertyChangedEventArgs(string? propertyName, object item) : base(propertyName) => Item = item;

    public object Item { get; }
}