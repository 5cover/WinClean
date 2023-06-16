using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page1AViewModel : WizardPageViewModel
{
    public Page1AViewModel()
    {
        AsyncRelayCommand start = new(CreateRestorePoint);
        EnterCommand = start;
        LeaveCommand = new RelayCommand(start.Cancel);
    }

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