using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel;

public sealed class DriveInfoViewModel : ObservableObject
{
    private bool isSelected;

    public DriveInfoViewModel(DriveInfo driveInfo) => Drive = driveInfo;

    public DriveInfo Drive { get; }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            var systemRestore = ServiceProvider.Get<IOperatingSystem>();
            isSelected = value;
            if (value)
            {
                systemRestore.EnableSystemRestore(Drive);
            }
            else
            {
                systemRestore.DisableSystemRestore(Drive);
            }
            OnPropertyChanged();
        }
    }
}