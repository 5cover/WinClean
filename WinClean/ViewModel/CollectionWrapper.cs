using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Scover.WinClean.ViewModel;

/// <summary>Type-safe wrapper over <see cref="ICollectionView"/>.</summary>
public sealed class CollectionWrapper<TSource, TItem> : ObservableObject, IEnumerable<TItem> where TSource : IEnumerable<TItem>
{
    private readonly CollectionViewSource _collectionViewSource;

    public CollectionWrapper(CollectionViewSource collectionViewSource)
    {
        Debug.Assert(collectionViewSource.Source is TSource);
        _collectionViewSource = collectionViewSource;
        View.CurrentChanged += (_, _) => OnPropertyChanged(nameof(CurrentItem));
        View.CurrentChanging += (_, _) => OnPropertyChanging(nameof(CurrentItem));
    }

    public CollectionWrapper(TSource collection) : this(new CollectionViewSource { Source = collection })
    {
    }

    public TItem? CurrentItem => (TItem?)View.CurrentItem;
    public TSource Source => (TSource)_collectionViewSource.Source;
    public ICollectionView View => _collectionViewSource.View;

    public IEnumerator<TItem> GetEnumerator() => Source.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Source.GetEnumerator();
}