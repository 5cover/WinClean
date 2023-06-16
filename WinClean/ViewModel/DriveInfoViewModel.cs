using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel;

public sealed class DriveInfoViewModel : ObservableObject
{
    private bool _isSelected;

    public DriveInfoViewModel(DriveInfo drive)
    {
        Drive = drive;
        IsSelected = false;
    }

    public DriveInfo Drive { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value)
            {
                OperatingSystem.EnableSystemRestore(Drive);
                Logs.SystemRestoreEnabledForDrive.FormatWith(Drive).Log(LogLevel.Info);
            }
            else
            {
                OperatingSystem.DisableSystemRestore(Drive);
                Logs.SystemRestoreDisabledForDrive.FormatWith(Drive).Log(LogLevel.Info);
            }
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    private static IOperatingSystem OperatingSystem => ServiceProvider.Get<IOperatingSystem>();
}