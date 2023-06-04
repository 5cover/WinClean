using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionInfoListViewModel : ObservableObject
{
    public ExecutionInfoListViewModel(ICollectionView executionInfos)
        => ExecutionInfos = executionInfos;

    public event EventHandler SelectionChanged { add => ExecutionInfos.CurrentChanged += value; remove => ExecutionInfos.CurrentChanged -= value; }

    public ExecutionInfoViewModel? CurrentItem => (ExecutionInfoViewModel?)ExecutionInfos.CurrentItem;

    public ICollectionView ExecutionInfos { get; }
}