using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Pages;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class ScriptExecutionWizardViewModel : ObservableObject
{
    public ScriptExecutionWizardViewModel(IReadOnlyList<ExecutionInfoViewModel> executionInfos)
    {
        CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfosWrapper = new(executionInfos);
        Page2ViewModel = new(executionInfosWrapper);
        Page3ViewModel = new(executionInfosWrapper);
    }

    public static TextStyle MainInstruction => ServiceProvider.Get<IThemeProvider>().MainInstruction;
    public Page1ViewModel Page1ViewModel { get; } = new();
    public Page2ViewModel Page2ViewModel { get; }
    public Page3ViewModel Page3ViewModel { get; }
}