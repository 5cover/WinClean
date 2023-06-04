using CommunityToolkit.Mvvm.ComponentModel;

namespace Scover.WinClean.ViewModel.Pages;

public partial class WizardPageViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _canSelectNextPage;

    /// <summary>
    /// Indicates that a page has finised and that the wizard should navigate to the next page.
    /// </summary>
    public event EventHandler? Finished;

    protected void OnFinished() => Finished?.Invoke(this, EventArgs.Empty);
}