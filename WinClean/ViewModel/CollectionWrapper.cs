using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace Scover.WinClean.ViewModel;

/// <summary>
/// Typesafe wrapper over <see cref="ICollectionView"/>.
/// </summary>
public sealed class CollectionWrapper<TSource, TItem> where TSource : IEnumerable<TItem>
{
    private readonly CollectionViewSource _collectionViewSource;
    public CollectionWrapper(CollectionViewSource collectionViewSource)
    {
        Debug.Assert(typeof(TSource).IsAssignableFrom(collectionViewSource.Source.GetType()));
        _collectionViewSource = collectionViewSource;
    }

    public CollectionWrapper(TSource collection) => _collectionViewSource = new() { Source = collection };

    public TSource Source => (TSource)_collectionViewSource.Source;
    public TItem CurrentItem => (TItem)View.CurrentItem;
    public ICollectionView View => _collectionViewSource.View;
}