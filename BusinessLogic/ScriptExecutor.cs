using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

public class ScriptExecutor
{
    private readonly Progress<ScriptExecutionProgressChangedEventArgs> _progress = new();
    private CancellationTokenSource? _cts;

    /// <summary>Occurs when a script has been executed.</summary>
    public event EventHandler<ScriptExecutionProgressChangedEventArgs> ProgressChanged { add => _progress.ProgressChanged += value; remove => _progress.ProgressChanged -= value; }

    /// <summary>Cancels script execution.</summary>
    /// <exception cref="InvalidOperationException">Scripts are not executing</exception>
    public void CancelScriptExecution()
    {
        if (_cts is null)
        {
            throw new InvalidOperationException(Resources.DevException.ScriptsAreNotExecuting);
        }
        _cts.Cancel(true);
    }

    /// <summary>Executes a list of scripts asynchronously. Raises the <see cref="ProgressChanged"/> event.</summary>
    /// <param name="scripts">The scripts to execute.</param>
    /// <inheritdoc cref="Script.Execute(HungScriptCallback, CancellationToken?)" path="/param"/>
    public async Task ExecuteScriptsAsync(IReadOnlyList<Script> scripts, HungScriptCallback keepRunningOrKill)
    {
        _cts = new();
        await Task.Run(() =>
        {
            for (int scriptIndex = 0; scriptIndex < scripts.Count && !_cts.IsCancellationRequested; ++scriptIndex)
            {
                ReportProgress();
                scripts[scriptIndex].Execute(keepRunningOrKill, _cts);

                void ReportProgress() => ((IProgress<ScriptExecutionProgressChangedEventArgs>)_progress).Report(new(scriptIndex));
            }
        }, _cts.Token).ConfigureAwait(false);
    }
}