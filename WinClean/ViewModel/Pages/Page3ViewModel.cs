using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page3ViewModel : WizardPageViewModel
{
    public Page3ViewModel(CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfos) => ExecutionInfos = executionInfos;

    public CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> ExecutionInfos { get; }

    public string FormattedDescription => Page3.MsgDescription.FormatMessage(new()
    {
        ["scriptCount"] = ExecutionInfos.Source.Count,
        ["elapsedTime"] = ExecutionInfos.Source.Sum(s => s.Script.ExecutionTime.ValueOr(TimeSpan.Zero)),
    });

    public IRelayCommand Restart { get; } = new RelayCommand(() =>
    {
        Logs.SystemRestartInitiated.Log(LogLevel.Info);
        ServiceProvider.Get<IOperatingSystem>().RestartForOSReconfig(force: false);
    });
}