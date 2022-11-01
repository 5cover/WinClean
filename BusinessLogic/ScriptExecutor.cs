using System.Diagnostics;

using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

public sealed class ScriptExecutor
{
    private readonly Progress<ScriptExecutionProgressChangedEventArgs> _progress = new();
    private CancellationTokenSource? _cts;

    /// <summary>Occurs when a script has been executed.</summary>
    public event EventHandler<ScriptExecutionProgressChangedEventArgs> ProgressChanged { add => _progress.ProgressChanged += value; remove => _progress.ProgressChanged -= value; }

    /// <summary>Cancels script execution.</summary>
    /// <remarks>Will do nothing if script execution is not running.</remarks>
    public void CancelScriptExecution() => _cts?.Cancel(true);

    /// <summary>Executes a list of scripts asynchronously. Raises the <see cref="ProgressChanged"/> event.</summary>
    /// <param name="scripts">The scripts to execute.</param>
    /// <inheritdoc cref="Script.Execute(HungScriptCallback, CancellationToken)" path="/param"/>
    public async Task ExecuteScriptsAsync(IReadOnlyList<Script> scripts, HungScriptCallback keepRunningElseKill)
    {
        _cts = new();

        Stopwatch stopwatch = new();

        await Task.Run(() =>
        {
            for (int scriptIndex = 0; scriptIndex < scripts.Count && !_cts.IsCancellationRequested; ++scriptIndex)
            {
                stopwatch.Restart();

                ReportProgress();
                scripts[scriptIndex].Execute(keepRunningElseKill, _cts.Token);

                scripts[scriptIndex].ExecutionTime = stopwatch.Elapsed;

                void ReportProgress() => ((IProgress<ScriptExecutionProgressChangedEventArgs>)_progress).Report(new(scriptIndex));
            }
        }, _cts.Token).ConfigureAwait(false);

        _cts.Dispose();
    }
}