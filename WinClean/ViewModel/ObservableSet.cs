using System.Collections.ObjectModel;

namespace Scover.WinClean.ViewModel;

public sealed class ObservableSet<T> : ObservableCollection<T>
{
    public ObservableSet(IEnumerable<T> collection) : base(collection)
    {
    }

    protected override void InsertItem(int index, T item)
    {
        if (!Contains(item))
        {
            base.InsertItem(index, item);
        }
    }

    protected override void SetItem(int index, T item)
    {
        int i = IndexOf(item);
        // If the item isn't already present or if we're replacing the same item, then we don't have a duplicate
        if (i < 0 || i == index)
        {
            base.SetItem(index, item);
        }
    }
}