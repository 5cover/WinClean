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
    public async Task ExecuteScriptsAsync(IReadOnlyList<Script> scripts, HungScriptCallback keepRunningElseTerminate)
    {
        _cts.Dispose();
        _cts = new();

        Stopwatch stopwatch = new();

        await Task.Run(() =>
        {
            for (int scriptIndex = 0; scriptIndex < scripts.Count && !_cts.IsCancellationRequested; ++scriptIndex)
            {
                ((IProgress<ScriptExecutionProgressChangedEventArgs>)_progress).Report(new(scriptIndex));

                Script script = scripts[scriptIndex];

                stopwatch.Restart();

                script.Execute(keepRunningElseTerminate, _cts.Token);

                AppInfo.PersistentSettings.ScriptExecutionTimes[script.InvariantName] = stopwatch.Elapsed.ToString("c");
            }
        }, _cts.Token).ConfigureAwait(false);
    }
}