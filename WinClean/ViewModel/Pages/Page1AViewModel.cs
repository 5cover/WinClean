using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page1AViewModel : WizardPageViewModel
{
    public Page1AViewModel()
    {
        OnEnter = new AsyncRelayCommand(CreateRestorePoint);
        OnLeave = new RelayCommand(OnEnter.Cancel);
    }

    public IAsyncRelayCommand OnEnter { get; }
    public IRelayCommand OnLeave { get; }

    private async Task CreateRestorePoint(CancellationToken cancellationToken)
    {
        await Task.Run(()
            => ServiceProvider.Get<IOperatingSystem>().CreateRestorePoint(ServiceProvider.Get<IApplicationInfo>().Name,
                                                                          RestorePointType.ModifySettings,
                                                                          RestorePointEventType.BeginSystemChange), cancellationToken);
        Logs.RestorePointCreated.Log(LogLevel.Info);
        OnFinished();
    }
}