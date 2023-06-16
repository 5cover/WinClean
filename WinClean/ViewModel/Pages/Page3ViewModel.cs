using CommunityToolkit.Mvvm.Input;

using Humanizer.Localisation;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page3ViewModel : WizardPageViewModel
{
    public Page3ViewModel(CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfos) => ExecutionInfos = executionInfos;

    public CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> ExecutionInfos { get; }

    public string FormattedElapsedTime => ExecutionInfos.Source.Sum(s => s.Script.ExecutionTime.ValueOr(TimeSpan.Zero)).Humanize(precision: 3, minUnit: TimeUnit.Second);

    public IRelayCommand Restart { get; } = new RelayCommand(() =>
    {
        Logs.SystemRestartInitiated.Log(LogLevel.Info);
        ServiceProvider.Get<IOperatingSystem>().RestartForOSReconfig(false);
    });
}