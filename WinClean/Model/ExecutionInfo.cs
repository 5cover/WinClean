using System.ComponentModel;
using System.Diagnostics;

namespace Scover.WinClean.Model;

public enum ProcessOutputKind
{
    Error,
    Standard
}

public sealed record ProcessOutput(ProcessOutputKind Kind, string? Text);

/// <summary>Information about a script's execution.</summary>
public sealed class ExecutionInfo : IDisposable
{
    private readonly Process _hostProcess;
    private readonly Stopwatch _stopwatch = new();
    private readonly HostStartInfo _hostStartInfo;
    public ExecutionInfo(ScriptAction action, ISynchronizeInvoke? synchronizingObject = null)
    {
        _hostStartInfo = action.CreateHostStartInfo();
        _hostProcess = new()
        {
            EnableRaisingEvents = true,
            SynchronizingObject = synchronizingObject,
            StartInfo = new()
            {
                FileName = _hostStartInfo.Filename,
                Arguments = _hostStartInfo.Arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            }
        };
    }

    private event EventHandler? _executionFinished;

    public void Dispose()
    {
        if (_hostProcess.HasExited)
        {
            _hostProcess.Dispose();
            _hostStartInfo.Dispose();
        }
        else
        {
            _executionFinished = (s, e) =>
            {
                _hostProcess.Dispose();
                _hostStartInfo.Dispose();
            };
        }
    }

    public ExecutionResult Execute()
    {
        Debug.Assert(_hostProcess.Start());
        _stopwatch.Restart();
        _hostProcess.WaitForExit();
        _stopwatch.Stop();

        return new(_hostProcess.ExitCode, _stopwatch.Elapsed);
    }

    /// <summary>Executes the script asynchronously.</summary>
    public async Task<ExecutionResult> ExecuteAsync(TimeSpan timeout, Func<bool> hungScriptkeepRunningElseTerminate, IProgress<ProcessOutput> progress, CancellationToken cancellationToken = default)
    {
        _hostProcess.OutputDataReceived += OnOutputDataReceived;
        _hostProcess.ErrorDataReceived += OnErrorDataReceived;
        _hostProcess.Exited += OnExit;

        Debug.Assert(_hostProcess.Start());
        _hostProcess.BeginOutputReadLine();
        _hostProcess.BeginErrorReadLine();
        _stopwatch.Restart();

        // Register after starting in case of cancellation before starting
        using var reg = cancellationToken.Register(KillHost);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await _hostProcess.WaitForExitAsync(cancellationToken).WithTimeout(timeout);
                ExecutionResult result = new(_hostProcess.ExitCode, _stopwatch.Elapsed);
                _executionFinished?.Invoke(this, EventArgs.Empty);
                return result;
            }
            catch (TimeoutException)
            {
                if (!hungScriptkeepRunningElseTerminate())
                {
                    KillHost();
                    throw;
                }
            }
        }

        void OnErrorDataReceived(object s, DataReceivedEventArgs e) => progress.Report(new(ProcessOutputKind.Error, e.Data));
        void OnOutputDataReceived(object s, DataReceivedEventArgs e) => progress.Report(new(ProcessOutputKind.Standard, e.Data));
        void OnExit(object? s, EventArgs e)
        {
            _stopwatch.Stop();
            _hostProcess.OutputDataReceived -= OnOutputDataReceived;
            _hostProcess.ErrorDataReceived -= OnErrorDataReceived;
            _hostProcess.Exited -= OnExit;
        }
    }

    public void Pause() => _hostProcess.Suspend();

    public void Resume() => _hostProcess.Resume();

    private void KillHost() => _hostProcess.Kill(true);
}