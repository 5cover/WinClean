using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page1ViewModel : WizardPageViewModel
{
    private bool _crateRestorePoint;
    private bool _dontCreateRestorePoint;

    public bool CreateRestorePoint
    {
        get => _crateRestorePoint;
        set
        {
            _crateRestorePoint = value;
            OnPropertyChanged();
            CanSelectNextPage = value ^ DontCreateRestorePoint;
        }
    }

    public bool DontCreateRestorePoint
    {
        get => _dontCreateRestorePoint;
        set
        {
            _dontCreateRestorePoint = value;
            OnPropertyChanged();
            CanSelectNextPage = value ^ CreateRestorePoint;
        }
    }

    public IEnumerable<DriveInfoViewModel> SystemRestoreEligibleDrives { get; } = ServiceProvider.Get<IOperatingSystem>().SystemRestoreEligibleDrives.Select(drive => new DriveInfoViewModel(drive));
}