using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class SettingsViewModel : ObservableObject
{
    public SettingsViewModel() => Reset = new RelayCommand(() =>
    {
        Settings.Reset();
        StaticPropertyChanged?.Invoke(this, new(nameof(Settings)));
        Logs.SettingsReset.Log();
    });

    // This event is subscribed to automatically by the WPF framework
    public static event PropertyChangedEventHandler? StaticPropertyChanged;

    public static ISettings Settings => ServiceProvider.Get<ISettings>();
    public IRelayCommand Reset { get; }
}