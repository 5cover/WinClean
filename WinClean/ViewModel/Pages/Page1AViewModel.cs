using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page1AViewModel : WizardPageViewModel
{
    public Page1AViewModel()
    {
        Start = new AsyncRelayCommand(CreateRestorePoint);
        Cancel = new RelayCommand(Start.Cancel);
    }

    public IRelayCommand Cancel { get; }

    public IAsyncRelayCommand Start { get; }

    private async Task CreateRestorePoint(CancellationToken cancellationToken)
    {
        await Task.Run(()
            => ServiceProvider.Get<IOperatingSystem>().CreateRestorePoint(ServiceProvider.Get<IApplicationInfo>().Name,
                                                                                 RestorePointEventType.BeginSystemChange,
                                                                                 RestorePointType.ModifySettings), cancellationToken);
        Logs.RestorePointCreated.Log(LogLevel.Info);
        OnFinished();
    }
}