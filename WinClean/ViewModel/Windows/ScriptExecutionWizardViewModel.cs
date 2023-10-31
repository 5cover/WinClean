using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Pages;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class ScriptExecutionWizardViewModel : ObservableObject
{
    private readonly CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> _executionInfos;

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
}