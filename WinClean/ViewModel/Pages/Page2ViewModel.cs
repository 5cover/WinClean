using CommunityToolkit.Mvvm.Input;

using Humanizer.Localisation;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page2ViewModel : WizardPageViewModel
{
    private bool _executionPaused;
    private bool _restartWhenFinished;
    private int _scriptIndex = -1;
    private TimeSpan _timeRemaining;

    public Page2ViewModel(CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfos)
    {
        ExecutionInfos = executionInfos;
        ExecutionInfoListViewModel = new(executionInfos.View);
        Start = new AsyncRelayCommand(Combine(StartTimer, ExecuteScripts));
        Stop = new RelayCommand(Start.Cancel);

        Pause = new RelayCommand(() =>
        {
            _executionPaused = true;
            Pause!.NotifyCanExecuteChanged();
            Resume!.NotifyCanExecuteChanged();
            ExecutingExecutionInfo.AssertNotNull().Pause();
        }, () => !_executionPaused);

        Resume = new RelayCommand(() =>
        {
            ExecutingExecutionInfo.AssertNotNull().Resume();
            Resume!.NotifyCanExecuteChanged();
            Pause!.NotifyCanExecuteChanged();
            _executionPaused = false;
        }, () => _executionPaused);
    }

    public ExecutionInfoViewModel? ExecutingExecutionInfo => ScriptIndex == -1 || ScriptIndex == ExecutionInfos.Source.Count ? null : ExecutionInfos.Source[ScriptIndex];

    public string FormattedTimeRemaining => TimeRemaining.Humanize(precision: 3, minUnit: TimeUnit.Second);

    public IRelayCommand Pause { get; }

    public bool RestartWhenFinished
    {
        get => _restartWhenFinished;
        set
        {
            _restartWhenFinished = value;
            OnPropertyChanged();
        }
    }

    public IRelayCommand Resume { get; }

    public int ScriptIndex
    {
        get => _scriptIndex;
        private set
        {
            _scriptIndex = value;
            TimeRemaining = GetCumulatedExecutionTime(ExecutionInfos.Source.Skip(ScriptIndex).Select(e => e.Script));
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExecutingExecutionInfo));
            OnPropertyChanged(nameof(ScriptsRemaining));
        }
    }

    public CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> ExecutionInfos { get; }

    public int ScriptsRemaining => ExecutionInfos.Source.Count - ScriptIndex;

    public ExecutionInfoListViewModel ExecutionInfoListViewModel { get; }

    public IAsyncRelayCommand Start { get; }

    public IRelayCommand Stop { get; }

    private static TimeSpan TimerInterval => 1.Seconds();

    private TimeSpan TimeRemaining
    {
        get => _timeRemaining;
        set
        {
            _timeRemaining = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormattedTimeRemaining));
        }
    }

    private static T Combine<T>(params T[] delegates) where T : Delegate => (T)Delegate.Combine(delegates)!;

    private static TimeSpan GetCumulatedExecutionTime(IEnumerable<ScriptViewModel> scripts) => scripts.Aggregate(TimeSpan.Zero, (sumSoFar, next) =>
    {
        var settings = ServiceProvider.Get<ISettings>();
        return sumSoFar + (settings.ScriptExecutionTimes.TryGetValue(next.InvariantName, out TimeSpan t) ? t : settings.ScriptTimeout);
    });

    private async Task ExecuteScripts(CancellationToken cancellationToken)
    {
        ScriptIndex = 0;
        foreach (var executionInfo in ExecutionInfos.Source)
        {
            try
            {
                executionInfo.Script.ExecutionTime = (await executionInfo.ExecuteAsync(cancellationToken)).ExecutionTime;
            }
            catch (OperationCanceledException)
            {
                // User canceled excecution
                return;
            }
            catch (TimeoutException)
            {
                // User terminated hung script, keep going.
            }
            ++ScriptIndex;
        }
        if (RestartWhenFinished)
        {
            Logs.SystemRestartInitiated.Log(LogLevel.Info);
            ServiceProvider.Get<IOperatingSystem>().RestartForOSReconfig(true);
        }
        else
        {
            OnFinished();
        }
    }

    private async Task StartTimer(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimerInterval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (TimeRemaining < TimerInterval)
            {
                TimeRemaining = TimeSpan.Zero;
            }
            else
            {
                TimeRemaining -= TimerInterval;
            }
        }
    }
}