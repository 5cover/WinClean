using CommunityToolkit.Mvvm.Input;

using Humanizer.Localisation;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page3ViewModel : WizardPageViewModel
{
    public Page3ViewModel(CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfos)
    {
        ExecutionInfos = executionInfos;
        foreach (var executionInfo in ExecutionInfos)
        {
            executionInfo.Script.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ScriptViewModel.ExecutionTime))
                {
                    OnPropertyChanged(nameof(FormattedElapsedTime));
                }
            };
        }
    }

    public CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> ExecutionInfos { get; }

    public string FormattedElapsedTime
        => ExecutionInfos.Source.Select(s => s.Script.ExecutionTime).AggregateOrNone(TimeSpan.Zero, (sumSoFar, t) => sumSoFar + t)
        .Match(elapsedTime => elapsedTime.Humanize(precision: 3, minUnit: TimeUnit.Second), () => Script.TimeSpanUnknown);

    public IRelayCommand Restart { get; } = new RelayCommand(() =>
    {
        Logs.SystemRestartInitiated.Log(LogLevel.Info);
        ServiceProvider.Get<IOperatingSystem>().RestartForOSReconfig(false);
    });
}