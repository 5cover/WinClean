using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Scover.WinClean;
using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class AboutViewModel : ObservableObject
{
    public static IRelayCommand OpenRepository => new RelayCommand(ServiceProvider.Get<IApplicationInfo>().RepositoryUrl.Open);

    public static string ApplicationName => ServiceProvider.Get<IApplicationInfo>().Name;
    public static string ApplicationVersion => ServiceProvider.Get<IApplicationInfo>().Version;
}