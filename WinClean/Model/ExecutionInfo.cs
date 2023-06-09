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
    private readonly HostStartInfo _hostStartInfo;
    private readonly Stopwatch _stopwatch = new();

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

    public void Abort()
    {
        if (!_hostProcess.IsRunning())
        {
            throw new InvalidOperationException("Host process is not executing. Execution is not underway.");
        }
        _hostProcess.Kill(true);
    }

    public void Dispose()
    {
        if (!_hostProcess.IsRunning())
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
        DenyIfExecuting();

        _ = _hostProcess.Start();
        _stopwatch.Restart();
        _hostProcess.WaitForExit();
        _stopwatch.Stop();

        return new(_hostProcess.ExitCode, _stopwatch.Elapsed);
    }

    /// <summary>Executes the script asynchronously.</summary>
    public async Task<ExecutionResult> ExecuteAsync(IProgress<ProcessOutput> progress, CancellationToken cancellationToken = default)
    {
        DenyIfExecuting();

        _hostProcess.OutputDataReceived += OnOutputDataReceived;
        _hostProcess.ErrorDataReceived += OnErrorDataReceived;

        try
        {
            _ = _hostProcess.Start();
            _hostProcess.BeginOutputReadLine();
            _hostProcess.BeginErrorReadLine();

            _stopwatch.Restart();

            // Register after starting in case of cancellation before starting
            // Note: Win32Exceptions for Access denied errors will be thrown by the Process class. They are
            // handled internally, so it's not a problem, but they're still visible by the debugger. This is
            // because the Process class enumerates all system processes to build the process tree, but
            // doesn't have permission to open all of them.
            using var reg = cancellationToken.Register(() => _hostProcess.Kill(true));

            await _hostProcess.WaitForExitAsync(cancellationToken);
        }
        finally
        {
            _stopwatch.Stop();
            _hostProcess.OutputDataReceived -= OnOutputDataReceived;
            _hostProcess.ErrorDataReceived -= OnErrorDataReceived;
        }

        ExecutionResult result = new(_hostProcess.ExitCode, _stopwatch.Elapsed);

        _executionFinished?.Invoke(this, EventArgs.Empty);
        return result;

        void OnErrorDataReceived(object s, DataReceivedEventArgs e) => progress.Report(new(ProcessOutputKind.Error, e.Data));
        void OnOutputDataReceived(object s, DataReceivedEventArgs e) => progress.Report(new(ProcessOutputKind.Standard, e.Data));
    }

    public void Pause()
    {
        foreach (var p in _hostProcess.GetProcessTree())
        {
            p.Suspend();
        }
    }

    public void Resume()
    {
        foreach (var p in _hostProcess.GetProcessTree())
        {
            p.Resume();
        }
    }

    private void DenyIfExecuting()
    {
        if (_hostProcess.IsRunning())
        {
            throw new InvalidOperationException("Host process is already executing. Execution may already be underway.");
        }
    }
}