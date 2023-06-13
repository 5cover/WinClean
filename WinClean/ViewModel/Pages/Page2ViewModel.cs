using CommunityToolkit.Mvvm.Input;

using Humanizer.Localisation;

using Optional;

using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed class Page2ViewModel : WizardPageViewModel
{
    private bool _executionPaused;
    private bool _restartWhenFinished;
    private int _scriptIndex = -1;
    private Option<TimeSpan> _timeRemaining;

    public Page2ViewModel(CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfos)
    {
        ExecutionInfos = executionInfos;
        Start = new AsyncRelayCommand(async ct =>
        {
            try
            {
                _ = StartTimer(ct); // fire & forget
                await ExecuteScripts(ct);
            }
            catch (OperationCanceledException)
            {
                // Make sure this doesn't go unhandled if a View isn't there to swallow the exception.
            }
        });
        Stop = new RelayCommand(Start.Cancel);

        AbortScript = new RelayCommand(() =>
        {
            if (DialogFactory.ShowConfirmation(DialogFactory.MakeConfirmAbortOperation))
            {
                ExecutingExecutionInfo.NotNull().Abort();
            }
        }, () => ExecutingExecutionInfo is not null);

        Pause = new RelayCommand(() =>
        {
            ExecutingExecutionInfo.NotNull().Pause();
            ExecutionPaused = true;
        }, () => ExecutingExecutionInfo is not null && !ExecutionPaused);
        Resume = new RelayCommand(() =>
        {
            ExecutingExecutionInfo.NotNull().Resume();
            ExecutionPaused = false;
        }, () => ExecutingExecutionInfo is not null && ExecutionPaused);
    }

    public IRelayCommand AbortScript { get; }
    public ExecutionInfoViewModel? ExecutingExecutionInfo => ScriptIndex == -1 || ScriptIndex == ExecutionInfos.Source.Count ? null : ExecutionInfos.Source[ScriptIndex];
    public CollectionWrapper<IList<ExecutionInfoViewModel>, ExecutionInfoViewModel> ExecutionInfos { get; }

    public bool ExecutionPaused
    {
        get => _executionPaused;
        private set
        {
            _executionPaused = value;
            OnPropertyChanged();
            Pause.NotifyCanExecuteChanged();
            Resume.NotifyCanExecuteChanged();
        }
    }

    public string FormattedTimeRemaining => TimeRemaining.Match(t => t.Humanize(precision: 3, minUnit: TimeUnit.Second), () => Script.TimeSpanUnknown);
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
            AbortScript.NotifyCanExecuteChanged();
            Pause.NotifyCanExecuteChanged();
            Resume.NotifyCanExecuteChanged();
        }
    }

    public int ScriptsRemaining => ExecutionInfos.Source.Count - ScriptIndex;
    public IAsyncRelayCommand Start { get; }
    public IRelayCommand Stop { get; }
    private static TimeSpan TimerInterval => 1.Seconds();

    private Option<TimeSpan> TimeRemaining
    {
        get => _timeRemaining;
        set
        {
            _timeRemaining = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormattedTimeRemaining));
        }
    }

    private static Option<TimeSpan> GetCumulatedExecutionTime(IEnumerable<ScriptViewModel> scripts) => scripts.Aggregate(TimeSpan.Zero.Some(), (sumSoFar, next)
        => sumSoFar.FlatMap(t => next.ExecutionTime.Map(et => et + t)));

    private async Task ExecuteScripts(CancellationToken cancellationToken)
    {
        ScriptIndex = 0;

        foreach (var executionInfo in ExecutionInfos.Source)
        {
            executionInfo.Result = await executionInfo.ExecuteAsync(cancellationToken);
            executionInfo.Script.ExecutionTime = executionInfo.Result.ExecutionTime.Some();
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
            if (ExecutionPaused)
            {
                continue;
            }

            // Prevent TimeRemaining from becoming negative. If we reach zero then it means we
            // underestimated execution time.
            TimeRemaining = TimeRemaining.FlatMap(t => t > TimerInterval
                ? (t - TimerInterval).Some()
                : Option.None<TimeSpan>());
        }
    }
}