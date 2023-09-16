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
    private readonly Lazy<Process> _hostProcess;
    private readonly HostStartInfo _hostStartInfo;
    private readonly Stopwatch _stopwatch = new();
    private bool _disposed;

    public ExecutionInfo(ScriptAction action, ISynchronizeInvoke? synchronizingObject = null)
    {
        _hostStartInfo = action.CreateHostStartInfo();
        _hostProcess = new(() => new Process()
        {
            EnableRaisingEvents = true,
            SynchronizingObject = synchronizingObject,
            StartInfo = new()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = _hostStartInfo.Filename,
                Arguments = _hostStartInfo.Arguments,
            }
        });
    }

    private Process HostProcess => _hostProcess.Value;
    private bool IsExecuting => _hostProcess.IsValueCreated && HostProcess.IsRunning();

    public void Abort()
    {
        DenyIfDisposed();
        DenyIfNotExecuting();

        HostProcess.KillTree();
    }

    public void Dispose()
    {
        _hostStartInfo.Dispose();
        _hostProcess.DisposeIfCreated();
        _disposed = true;
    }

    public ExecutionResult Execute()
    {
        DenyIfDisposed();
        DenyIfExecuting();

        _ = HostProcess.Start();
        _stopwatch.Restart();
        HostProcess.WaitForExit();
        _stopwatch.Stop();

        return new(HostProcess.ExitCode, _stopwatch.Elapsed);
    }

    /// <summary>Executes the script asynchronously.</summary>
    public async Task<ExecutionResult> ExecuteAsync(IProgress<ProcessOutput> progress, CancellationToken cancellationToken = default)
    {
        DenyIfDisposed();
        DenyIfExecuting();

        HostProcess.OutputDataReceived += OnOutputDataReceived;
        HostProcess.ErrorDataReceived += OnErrorDataReceived;

        try
        {
            _ = HostProcess.Start();
            HostProcess.BeginOutputReadLine();
            HostProcess.BeginErrorReadLine();

            _stopwatch.Restart();

            // Register after starting in case of cancellation before starting
            using var reg = cancellationToken.Register(HostProcess.KillTree);

            await HostProcess.WaitForExitAsync(cancellationToken);
        }
        finally
        {
            _stopwatch.Stop();
            HostProcess.OutputDataReceived -= OnOutputDataReceived;
            HostProcess.ErrorDataReceived -= OnErrorDataReceived;
        }

        ExecutionResult result = new(HostProcess.ExitCode, _stopwatch.Elapsed);

        return result;

        void OnErrorDataReceived(object s, DataReceivedEventArgs e) => progress.Report(new(ProcessOutputKind.Error, e.Data));
        void OnOutputDataReceived(object s, DataReceivedEventArgs e) => progress.Report(new(ProcessOutputKind.Standard, e.Data));
    }

    public void Pause()
    {
        DenyIfDisposed();
        DenyIfNotExecuting();

        foreach (var p in HostProcess.GetProcessTree())
        {
            p.Suspend();
        }
        _stopwatch.Stop();
    }

    public void Resume()
    {
        DenyIfDisposed();
        DenyIfNotExecuting();

        _stopwatch.Start();
        foreach (var p in HostProcess.GetProcessTree())
        {
            p.Resume();
        }
    }

    private void DenyIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(ToString());
        }
    }

    private void DenyIfExecuting()
    {
        if (IsExecuting)
        {
            throw new InvalidOperationException("Host process is already executing. Execution may already be underway.");
        }
    }

    private void DenyIfNotExecuting()
    {
        if (!IsExecuting)
        {
            throw new InvalidOperationException("Host process is not executing. Execution is not underway.");
        }
    }
}