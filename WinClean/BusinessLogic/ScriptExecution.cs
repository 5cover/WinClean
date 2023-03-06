using System.Diagnostics;
using System.Text;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

public sealed class ScriptExecution : IDisposable
{
    private readonly StringBuilder _stdout = new(), _stderr = new(), _full = new();
    private readonly Process _process;

    public event EventHandler ProcessExited {add => _process.Exited += value; remove => _process.Exited -= value; }

    public ScriptExecution(ProcessStartInfo hostProcessStartInfo)
    {
        _process = Process.Start(hostProcessStartInfo).AssertNotNull();
        _process.OutputDataReceived += (s, e) =>
        {
            _ = _stdout.Append(e.Data);
            _ = _full.Append(e.Data);
        };
        _process.ErrorDataReceived += (s, e) =>
        {
            _ = _stderr.Append(e.Data);
            _ = _full.Append(e.Data);
        };
    }

    public async Task<int> WaitForExitAsync(TimeSpan timeout, Func<bool> hungScriptkeepRunningElseTerminate, CancellationToken cancellationToken)
    {
        while (!_process.HasExited)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await _process.WaitForExitAsync(cancellationToken).WithTimeout(timeout);
            }
            catch (TimeoutException)
            {
                if (!hungScriptkeepRunningElseTerminate())
                {
                    _process.Kill(true);
                    throw;
                }
            }
        }
        return _process.ExitCode;
    }

    public int WaitForExit()
    {
        _process.WaitForExit();
        return _process.ExitCode;
    }

    public string StandardError => _stderr.ToString();

    public string StandardOuput => _stdout.ToString();

    public string FullOutput => _full.ToString();

    public void Dispose() => _process.Dispose();
}