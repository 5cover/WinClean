using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page1ViewModel : WizardPageViewModel
{
    public static IRelayCommand OpenSystemProtectionSettings { get; } = new RelayCommand(()
        => ServiceProvider.Get<IOperatingSystem>().OpenSytemPropertiesProtection());
}