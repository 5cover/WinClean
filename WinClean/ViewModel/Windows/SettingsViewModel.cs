using System.Diagnostics.CodeAnalysis;

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
        OnPropertyChanged(nameof(Settings));
        Logs.SettingsReset.Log();
    });

    public IRelayCommand Reset { get; }

    [SuppressMessage("Performance", "CA1822", Justification = "Binding wouldn't update on PropertyChanged")]
    public ISettings Settings => ServiceProvider.Get<ISettings>();
}