using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Optional;

using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel.Pages;

public sealed partial class Page2ViewModel : WizardPageViewModel
{
    private bool _executionPaused;
    private string _formattedTimeRemaining = TimeRemaining.Unknown;

    [ObservableProperty]
    private bool _restartWhenFinished;

    private int _scriptIndex = -1;

    public Page2ViewModel(CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> executionInfos)
    {
        ExecutionInfos = executionInfos;

        AsyncRelayCommand start = new(async ct =>
        {
            Logs.StartedScriptExecution.FormatWith(ExecutionInfos.Source.Count).Log();
            try
            {
                await ExecuteScripts(ct);
            }
            catch (OperationCanceledException)
            {
                Logs.CanceledScriptExecution.Log();
                // Make sure this doesn't go unhandled.
            }
            Logs.FinishedScriptExecution.Log();
        });
        EnterCommand = start;
        LeaveCommand = new RelayCommand(start.Cancel);

        Stop = new RelayCommand(OnClosingRequested);

        AbortScript = new RelayCommand(() =>
        {
            if (DialogFactory.ShowConfirmation(DialogFactory.MakeConfirmAbortOperation))
            {
                // Might be null if the script finishes execution while the confirmation dialog is shown.
                ExecutingExecutionInfo?.Abort();
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

    public static Brush PausedProgressBarBrush => ServiceProvider.Get<IThemeProvider>().PausedProgressBarBrush;
    public IRelayCommand AbortScript { get; }
    public ExecutionInfoViewModel? ExecutingExecutionInfo => ScriptIndex == -1 || ScriptIndex == ExecutionInfos.Source.Count ? null : ExecutionInfos.Source[ScriptIndex];
    public CollectionWrapper<IReadOnlyList<ExecutionInfoViewModel>, ExecutionInfoViewModel> ExecutionInfos { get; }

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

    public string FormattedProgress => Page2.MsgProgress.FormatMessage(new()
    {
        ["executedCount"] = ScriptIndex,
        ["scriptCount"] = ExecutionInfos.Source.Count,
        ["remainingCount"] = ExecutionInfos.Source.Count - ScriptIndex,
        ["currentScriptName"] = ExecutingExecutionInfo?.Script.Name,
        ["remainingTime"] = FormattedTimeRemaining,
    });

    public string FormattedTimeRemaining
    {
        get => _formattedTimeRemaining;
        private set
        {
            _formattedTimeRemaining = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormattedProgress));
        }
    }

    public IRelayCommand Pause { get; }
    public IRelayCommand Resume { get; }

    public int ScriptIndex
    {
        get => _scriptIndex;
        private set
        {
            _scriptIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FormattedProgress));
            FormattedTimeRemaining = FormatTimeRemaining(ExecutionInfos.Source.Skip(ScriptIndex).Select(e => e.Script.ExecutionTime));
            AbortScript.NotifyCanExecuteChanged();
            Pause.NotifyCanExecuteChanged();
            Resume.NotifyCanExecuteChanged();
        }
    }

    public IRelayCommand Stop { get; }

    public static string FormatTimeRemaining(IEnumerable<Option<TimeSpan>> durations)
    {
        int knownCount = 0;
        int unknownCount = 0;
        TimeSpan timeRemaining = durations.Aggregate(TimeSpan.Zero, (soFar, newVal) =>
        {
            if (newVal.HasValue)
            {
                ++knownCount;
            }
            else
            {
                ++unknownCount;
            }
            return soFar + newVal.ValueOr(TimeSpan.Zero);
        });

        return knownCount == 0
            ? TimeRemaining.Unknown
            : unknownCount > 0
            ? TimeRemaining.AtLeast.FormatWith(timeRemaining.HumanizeToSeconds())
            : timeRemaining.HumanizeToSeconds();
    }

    private async Task ExecuteScripts(CancellationToken cancellationToken)
    {
        ScriptIndex = 0;

        foreach (var executionInfo in ExecutionInfos.Source)
        {
            await executionInfo.ExecuteAsync(cancellationToken);
            ++ScriptIndex;
        }

        if (RestartWhenFinished)
        {
            Logs.SystemRestartInitiated.Log(LogLevel.Info);
            ServiceProvider.Get<IOperatingSystem>().RestartForOSReconfig(force: true);
        }
        else
        {
            OnFinished();
        }
    }
}