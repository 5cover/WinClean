using System.Windows;
using System.Windows.Controls.Primitives;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

using ExecutionResult = Scover.WinClean.Model.ExecutionResult;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionInfoViewModel : ObservableObject
{
    private readonly ExecutionInfo _model;
    private ExecutionResult? _result;
    private bool _userIsNotScrolling = true;

    public ExecutionInfoViewModel(ScriptViewModel script, Capability capabilityToExecute, ScriptAction actionToExecute)
    {
        Script = script;
        Capability = capabilityToExecute;
        _model = new(actionToExecute, Application.Current.GetSynchronizationObject());
        NotifyScroll = new RelayCommand<ScrollEventArgs>(e => UserIsNotScrolling = e.AssertNotNull().ScrollEventType is ScrollEventType.EndScroll);
    }

    public ScriptViewModel Script { get; }
    public Capability Capability { get; }
    public ExecutionProgressViewModel Progress { get; } = new();

    public ExecutionResult? Result
    {
        get => _result;
        set
        {
            _result = value;
            OnPropertyChanged();
        }
    }

    public IRelayCommand<ScrollEventArgs> NotifyScroll { get; }

    public bool UserIsNotScrolling
    {
        get => _userIsNotScrolling;
        private set
        {
            _userIsNotScrolling = value;
            OnPropertyChanged();
        }
    }

    public void Dispose() => _model.Dispose();

    /// <inheritdoc cref="ExecutionInfo.ExecuteAsync(TimeSpan, Func{bool}, IProgress{ProcessOutput}, CancellationToken)"/>
    /// <remarks>Sets <see cref="Result"/>.</remarks>
    /// <returns><see cref="Result"/>.</returns>
    public async Task<ExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
        => Result = await _model.ExecuteAsync(ServiceProvider.Get<ISettings>().ScriptTimeout, HandleHungScript, Progress, cancellationToken);

    public void Pause() => _model.Pause();

    public void Resume() => _model.Resume();

    private bool HandleHungScript()
    {
        Logs.HungScript.FormatWith(Script.InvariantName, ServiceProvider.Get<ISettings>().ScriptTimeout).Log(LogLevel.Warning);
        Button endTask = new(Buttons.EndTask);
        using Page page = new()
        {
            IsCancelable = true,
            WindowTitle = ScriptExecution.WindowTitle.FormatWith(ServiceProvider.Get<IApplicationInfo>().Name),
            Icon = DialogIcon.Warning,
            Content = ScriptExecution.HungScriptDialogContent.FormatWith(Script.Name, ServiceProvider.Get<ISettings>().ScriptTimeout),
            Buttons = { endTask, Button.Ignore },
        };
        return new Dialog(page).Show() != endTask;
    }
}