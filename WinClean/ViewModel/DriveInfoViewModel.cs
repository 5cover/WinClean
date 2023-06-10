using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel;

public sealed class DriveInfoViewModel : ObservableObject
{
    private bool _isSelected;

    public DriveInfoViewModel(DriveInfo driveInfo) => Drive = driveInfo;

    public DriveInfo Drive { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            var systemRestore = ServiceProvider.Get<IOperatingSystem>();
            if (value)
            {
                systemRestore.EnableSystemRestore(Drive);
                Logs.SystemRestoreEnabledForDrive.FormatWith(Drive).Log(LogLevel.Info);
            }
            else
            {
                systemRestore.DisableSystemRestore(Drive);
                Logs.SystemRestoreDisabledForDrive.FormatWith(Drive).Log(LogLevel.Info);
            }
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}