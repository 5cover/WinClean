using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Optional;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel;

public enum ScriptExecutionState
{
    Finished,
    Paused,
    Pending,
    Running,
    Skipped,
}

[DebuggerDisplay($"{nameof(State)}: {{{nameof(State)}}}")]
public sealed class ExecutionInfoViewModel : ObservableObject, IDisposable
{
    private static readonly Lazy<ISynchronizeInvoke> synchronizationObject = new(Application.Current.CreateSynchronizationObject);
    private readonly ExecutionInfo _model;
    private ExecutionResultViewModel? _result;
    private ScriptExecutionState _state = ScriptExecutionState.Pending;
    private bool _userIsNotScrolling = true;

    public ExecutionInfoViewModel(ScriptViewModel script, Capability capabilityToExecute, ScriptAction actionToExecute)
    {
        Script = script;
        Action = actionToExecute;
        Capability = capabilityToExecute;
        _model = new(actionToExecute, synchronizationObject.Value);
        NotifyScroll = new RelayCommand<ScrollEventArgs>(e => UserIsNotScrolling = e.NotNull().ScrollEventType is ScrollEventType.EndScroll);
        FormattedEstimatedExecutionTime = script.ExecutionTime.Match(t => t.HumanizeToMilliseconds(), () => TimeRemaining.Unknown);
    }

    public Capability Capability { get; }

    public ScriptAction Action { get; }

    public string FormattedEstimatedExecutionTime { get; }

    public IRelayCommand<ScrollEventArgs> NotifyScroll { get; }

    public ExecutionProgressViewModel Progress { get; } = new();

    public ExecutionResultViewModel? Result
    {
        get => _result;
        private set
        {
            _result = value;
            OnPropertyChanged();
        }
    }

    public ScriptViewModel Script { get; }

    public ScriptExecutionState State
    {
        get => _state;
        private set
        {
            _state = value;
            OnPropertyChanged();
        }
    }

    public bool UserIsNotScrolling
    {
        get => _userIsNotScrolling;
        private set
        {
            _userIsNotScrolling = value;
            OnPropertyChanged();
        }
    }

    public void Abort()
    {
        _model.Abort();
        Format(Logs.ScriptExecutionAborted).Log(LogLevel.Info);
    }

    public void Dispose() => _model.Dispose();

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!await GetExecutionNeededAsync(cancellationToken))
        {
            State = ScriptExecutionState.Skipped;
            Logs.ScriptExecutionSkipped.FormatWith(Script.InvariantName, Capability.InvariantName).Log();
            Result = null;
            return;
        }

        using var reg = cancellationToken.Register(Abort);

        State = ScriptExecutionState.Running;
        Result = new(await _model.ExecuteAsync(Progress, cancellationToken));
        Logs.ScriptExecutionCompleted.FormatWith(Script.InvariantName, Capability.InvariantName, Result.ExitCode, Result.Succeeded).Log();
        State = ScriptExecutionState.Finished;

        if (Result.Succeeded)
        {
            Script.ExecutionTime = Result.ExecutionTime.Some();
        }
        // For aborted scripts, they might have changed system configuration before being aborted, that's why we still invalidate the cache.
        Script.Code.EffectiveCapability.InvalidateValue();
    }

    public void Pause()
    {
        _model.Pause();
        Format(Logs.ScriptExecutionPaused).Log();
        State = ScriptExecutionState.Paused;
    }

    public void Resume()
    {
        _model.Resume();
        Format(Logs.ScriptExecutionResumed).Log();
        State = ScriptExecutionState.Running;
    }

    private string Format(string message) => message.FormatWith(Script.InvariantName, Capability.InvariantName);

    private async Task<bool> GetExecutionNeededAsync(CancellationToken cancellationToken)
                      => ServiceProvider.Get<ISettings>().ForceExecuteEffectiveScripts ||
            !Capability.Equals(await Script.Code.EffectiveCapability.GetValueAsync(cancellationToken));
}