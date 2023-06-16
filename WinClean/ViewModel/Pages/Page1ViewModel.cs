using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page1ViewModel : WizardPageViewModel
{
    public bool CreateRestorePoint => SystemRestoreEligibleDrives.Any(d => d.IsSelected);

    public ICollection<DriveInfoViewModel> SystemRestoreEligibleDrives { get; } = ServiceProvider.Get<IOperatingSystem>().SystemRestoreEligibleDrives
        .Select(drive => new DriveInfoViewModel(drive)).ToList();
}