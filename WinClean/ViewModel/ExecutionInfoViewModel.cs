using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionInfoViewModel : ObservableObject
{
    private static readonly Lazy<ISynchronizeInvoke> synchronizationObject = new(Application.Current.CreateSynchronizationObject);
    private readonly ExecutionInfo _model;
    private ExecutionResultViewModel? _result;
    private bool _userIsNotScrolling = true;

    public ExecutionInfoViewModel(ScriptViewModel script, Capability capabilityToExecute, ScriptAction actionToExecute)
    {
        Script = script;
        Capability = capabilityToExecute;
        _model = new(actionToExecute, synchronizationObject.Value);
        NotifyScroll = new RelayCommand<ScrollEventArgs>(e => UserIsNotScrolling = e.NotNull().ScrollEventType is ScrollEventType.EndScroll);
        FormattedOriginalEstimatedExecutionTime = script.ExecutionTime.Match(t => t.FormatToSeconds(), () => Resources.Script.TimeSpanUnknown);
    }

    public Capability Capability { get; }

    public string FormattedOriginalEstimatedExecutionTime { get; }

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

    /// <inheritdoc cref="ExecutionInfo.ExecuteAsync(IProgress{ProcessOutput}, CancellationToken)"/>
    public async Task<ExecutionResultViewModel> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await _model.ExecuteAsync(Progress, cancellationToken);
        Logs.ScriptExecutionCompleted.FormatWith(Script.InvariantName, Capability.InvariantName, result.ExitCode, result.Succeeded).Log();
        return new(result);
    }

    public async Task<bool> GetExecutionNeededAsync(CancellationToken cancellationToken)
        => !Capability.Equals(await Script.Code.EffectiveCapability.GetValueAsync(cancellationToken));

    public void Pause()
    {
        _model.Pause();
        Format(Logs.ScriptExecutionPaused).Log();
    }

    public void Resume()
    {
        _model.Resume();
        Format(Logs.ScriptExecutionResumed).Log();
    }

    private string Format(string message) => message.FormatWith(Script.InvariantName, Capability.InvariantName);
}