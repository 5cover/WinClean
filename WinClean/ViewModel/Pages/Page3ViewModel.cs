using CommunityToolkit.Mvvm.Input;

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
        => ExecutionInfos.Source.Sum(s => s.Script.ExecutionTime.ValueOr(TimeSpan.Zero)).HumanizeToSeconds();

    public IRelayCommand Restart { get; } = new RelayCommand(() =>
    {
        Logs.SystemRestartInitiated.Log(LogLevel.Info);
        ServiceProvider.Get<IOperatingSystem>().RestartForOSReconfig(false);
    });
}