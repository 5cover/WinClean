using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scover.WinClean.ViewModel.Pages;

public partial class WizardPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _canSelectNextPage;

    [ObservableProperty]
    private IRelayCommand? _enterCommand;

    [ObservableProperty]
    private IRelayCommand? _leaveCommand;

    /// <summary>
    /// Indicates that a page has finised and that the wizard should navigate to the next page.
    /// </summary>
    public event TypeEventHandler<WizardPageViewModel>? Finished;

    /// <summary>
    /// Indicates that a page has requested cancelation of the wizard.
    /// </summary>
    public event TypeEventHandler<WizardPageViewModel>? ClosingRequested;

    protected void OnFinished() => Finished?.Invoke(this, EventArgs.Empty);
    protected void OnClosingRequested() => ClosingRequested?.Invoke(this, EventArgs.Empty);
}