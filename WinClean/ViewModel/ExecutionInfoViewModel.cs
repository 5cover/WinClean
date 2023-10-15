using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel;

public enum ScriptExecutionState
{
    Pending,
    Running,
    Paused,
    Finished,
}

[DebuggerDisplay($"{nameof(State)}: {{{nameof(State)}}}")]
public sealed class ExecutionInfoViewModel : ObservableObject
{
    private static readonly Lazy<ISynchronizeInvoke> synchronizationObject = new(Application.Current.CreateSynchronizationObject);
    private readonly ExecutionInfo _model;
    private ExecutionResultViewModel? _result;
    private ScriptExecutionState _state = ScriptExecutionState.Pending;
    private bool _userIsNotScrolling = true;

    public ExecutionInfoViewModel(ScriptViewModel script, Capability capabilityToExecute, ScriptAction actionToExecute)
    {
        Script = script;
        Capability = capabilityToExecute;
        _model = new(actionToExecute, synchronizationObject.Value);
        NotifyScroll = new RelayCommand<ScrollEventArgs>(e => UserIsNotScrolling = e.NotNull().ScrollEventType is ScrollEventType.EndScroll);
        FormattedEstimatedExecutionTime = script.ExecutionTime.Match(t => t.FormatToSeconds(), () => TimeRemaining.Unknown);
    }

    public Capability Capability { get; }

    public string FormattedEstimatedExecutionTime { get; }

    public IRelayCommand<ScrollEventArgs> NotifyScroll { get; }

    public ExecutionProgressViewModel Progress { get; } = new();

    public ExecutionResultViewModel? Result
    {
        get => _result;
        set
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

    public async Task<ExecutionResultViewModel> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        State = ScriptExecutionState.Running;
        var result = await _model.ExecuteAsync(Progress, cancellationToken);
        Logs.ScriptExecutionCompleted.FormatWith(Script.InvariantName, Capability.InvariantName, result.ExitCode, result.Succeeded).Log();
        State = ScriptExecutionState.Finished;
        return new(result);
    }

    public async Task<bool> GetExecutionNeededAsync(CancellationToken cancellationToken)
        => ServiceProvider.Get<ISettings>().ForceExecuteEffectiveScripts || !Capability.Equals(await Script.Code.EffectiveCapability.GetValueAsync(cancellationToken));

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
}