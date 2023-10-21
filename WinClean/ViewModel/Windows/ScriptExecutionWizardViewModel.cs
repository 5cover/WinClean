using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Pages;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class ScriptExecutionWizardViewModel : ObservableObject, IDisposable
{
    private readonly CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> _executionInfos;

    /// <remarks>
    /// <paramref name="executionInfos"/> is considered to be owned by this class and will be disposed by
    /// it.
    /// </remarks>
    public ScriptExecutionWizardViewModel(IReadOnlyList<ExecutionInfoViewModel> executionInfos)
    {
        _executionInfos = new(executionInfos);
        Page2ViewModel = new(_executionInfos);
        Page3ViewModel = new(_executionInfos);
    }

    public static TextStyle MainInstruction => ServiceProvider.Get<IThemeProvider>().MainInstruction;
    public Page1ViewModel Page1ViewModel { get; } = new();
    public Page2ViewModel Page2ViewModel { get; }
    public Page3ViewModel Page3ViewModel { get; }

    public void Dispose()
    {
        foreach (var executionInfo in _executionInfos)
        {
            executionInfo.Dispose();
        }
    }
}