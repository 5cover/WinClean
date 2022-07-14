using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

public class ScriptExecutor
{
    private readonly CancellationToken _ct;
    private readonly CancellationTokenSource _cts;
    private readonly Progress<ScriptExecutionProgressChangedEventArgs> _progress = new();

    public ScriptExecutor()
    {
        _cts = new();
        _ct = _cts.Token;
    }

    /// <summary>Occurs when a script has been executed.</summary>
    public event EventHandler<ScriptExecutionProgressChangedEventArgs> ProgressChanged { add => _progress.ProgressChanged += value; remove => _progress.ProgressChanged -= value; }

    public void CancelScriptExecution() => _cts.Cancel(true);

    /// <summary>Executes a list of scripts asynchronously. Raises the <see cref="ProgressChanged"/> event.</summary>
    /// <param name="scripts">The scripts to execute.</param>
    /// <inheritdoc cref="Script.Execute(HungScriptCallback)" path="/param"/>
    public async Task ExecuteScriptsAsync(IReadOnlyList<Script> scripts, HungScriptCallback keepRunningOrKill)
        => await Task.Run(() =>
        {
            for (int scriptIndex = 0; scriptIndex < scripts.Count && !_ct.IsCancellationRequested; ++scriptIndex)
            {
                scripts[scriptIndex].Execute(keepRunningOrKill, _ct);
                // Report the progress AFTER executing the script
                ((IProgress<ScriptExecutionProgressChangedEventArgs>)_progress).Report(new(scriptIndex));
            }
        }, _ct).ConfigureAwait(false);
}