using System.Diagnostics;

using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

public sealed class ScriptExecutor : IDisposable
{
    private readonly Progress<ScriptExecutionProgressChangedEventArgs> _progress = new();
    private CancellationTokenSource _cts = new();

    /// <summary>Occurs when a script has been executed.</summary>
    public event EventHandler<ScriptExecutionProgressChangedEventArgs> ProgressChanged { add => _progress.ProgressChanged += value; remove => _progress.ProgressChanged -= value; }

    /// <summary>Cancels script execution.</summary>
    /// <remarks>Will do nothing if script execution is not running.</remarks>
    public void CancelScriptExecution() => _cts.Cancel();

    public void Dispose() => _cts.Dispose();

    /// <summary>Executes a list of scripts asynchronously. Raises the <see cref="ProgressChanged"/> event.</summary>
    /// <param name="scripts">The scripts to execute.</param>
    /// <inheritdoc cref="Script.Execute" path="/param"/>
    /// <param name="keepRunningElseTerminate">
    /// <inheritdoc cref="HungScriptCallback" path="/summary"/> Returns <see langword="true"/> if the script should keep
    /// running, or <see langword="false"/> if its associated process tree should be terminated.
    /// </param>
    public async Task ExecuteScriptsAsync(IReadOnlyList<Script> scripts, HungScriptCallback keepRunningElseTerminate)
    {
        _cts.Dispose();
        _cts = new();

        Stopwatch stopwatch = new();

        await Task.Run(() =>
        {
            for (int scriptIndex = 0; scriptIndex < scripts.Count && !_cts.IsCancellationRequested; ++scriptIndex)
            {
                Script script = scripts[scriptIndex];
                // 1. Restart the elapsed time stopwatch.
                stopwatch.Restart();
                // 2. Execute the script.
                script.Execute(() =>
                {
                    if (!keepRunningElseTerminate(script))
                    {
                        CancelScriptExecution();
                    }
                }, _cts.Token);
                // 3. Update the container for script execution times.
                AppInfo.PersistentSettings.ScriptExecutionTimes[script.InvariantName] = stopwatch.Elapsed.ToString("c");
                // 4. Report the progress made to the caller.
                ((IProgress<ScriptExecutionProgressChangedEventArgs>)_progress).Report(new(scriptIndex));
            }
        }, _cts.Token).ConfigureAwait(false);
    }
}