namespace Scover.WinClean.Logic;

public class ScriptExecutor
{
    private readonly Progress<ScriptExecutionProgressChangedEventArgs> _progress = new();

    private readonly CancellationToken ct;

    private readonly CancellationTokenSource cts;

    public ScriptExecutor()
    {
        cts = new();
        ct = cts.Token;
    }

    /// <summary>Occurs when a script has been executed.</summary>
    public event EventHandler<ScriptExecutionProgressChangedEventArgs> ProgressChanged { add => _progress.ProgressChanged += value; remove => _progress.ProgressChanged -= value; }

    public void CancelScriptExecution() => cts.Cancel(true);

    /// <summary>Executes asynchronously a list of scripts. Raises the <see cref="ProgressChanged"/> event.</summary>
    /// <param name="scripts">The scripts to execute.</param>
    /// <returns>An awaitable task.</returns>
    public async Task ExecuteScriptsAsync(IReadOnlyList<Script> scripts, TimeSpan timeout, Func<string, bool> promptEndTaskOnHung, Func<Exception, FileSystemInfo, Operational.FSVerb, bool> promptRetryOnFSError)
        => await Task.Run(() =>
        {
            for (int scriptIndex = 0; scriptIndex < scripts.Count && !ct.IsCancellationRequested; ++scriptIndex)
            {
                scripts[scriptIndex].Execute(timeout, promptEndTaskOnHung, promptRetryOnFSError);
                // Report the progress AFTER executing the script
                ((IProgress<ScriptExecutionProgressChangedEventArgs>)_progress).Report(new(scriptIndex));
            }
        }, ct).ConfigureAwait(false);
}