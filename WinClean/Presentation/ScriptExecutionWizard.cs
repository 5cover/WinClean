using System.Diagnostics;

using Scover.Dialogs;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;

using Vanara.PInvoke;

using static Vanara.PInvoke.Shell32;

namespace Scover.WinClean.Presentation;

/// <summary>
/// Walks the user through the multi-step high-level operation of executing multiple scripts asynchronously
/// by displaying a dialog tracking the progress.
/// </summary>
public sealed partial class ScriptExecutionWizard : IDisposable
{
    private static readonly TimeSpan _timerInterval = 1.Seconds();
    private static readonly DialogIcon restorePointIcon, softwareIcon;
    private readonly Page _askRestartCompleted, _askRestorePoint, _executionProgress, _restorePointProgress;
    private readonly CancellationTokenSource _ctsScriptExecution = new();
    private readonly IReadOnlyList<Script> _scripts;
    private readonly PeriodicTimer _timer = new(_timerInterval);
    private int _scriptIndex;
    private TimeSpan _timeRemaining;

    static ScriptExecutionWizard()
    {
        ushort _ = 0;
        var restorePointIconHandle = ExtractAssociatedIcon(IntPtr.Zero, new(Path.Join(Environment.SystemDirectory, "rstrui.exe")), ref _);
        restorePointIconHandle.ScheduleDisposeOnExit();
        restorePointIcon = DialogIcon.FromHandle(restorePointIconHandle.DangerousGetHandle());

        var shellIconInfo = SHSTOCKICONINFO.Default;
        SHGetStockIconInfo(SHSTOCKICONID.SIID_SOFTWARE, SHGSI.SHGSI_ICON, ref shellIconInfo).ThrowIfFailed();
        var softwareIconHandle = (nint)shellIconInfo.hIcon;
        new User32.SafeHICON(softwareIconHandle).ScheduleDisposeOnExit();
        softwareIcon = DialogIcon.FromHandle(softwareIconHandle);
    }

    public void Dispose()
    {
        _askRestartCompleted.Dispose();
        _askRestorePoint.Dispose();
        _executionProgress.Dispose();
        _restorePointProgress.Dispose();
    }

    /// <summary>Executes the script(s) and displays a dialog tracking the progress.</summary>
    public void Execute() => new MultiPageDialog(_askRestorePoint, new()
    {
        [_askRestorePoint] = request =>
        {
            var txt = (request.ClickedButton as Button)?.Text;
            return txt == AskRestorePoint.CommandLinkYes
                     ? _restorePointProgress
                     : txt == AskRestorePoint.CommandLinkNo
                         ? _executionProgress
                         : null;
        },
        [_restorePointProgress] = request => request.Kind is NavigationRequestKind.Exit ? _executionProgress : null,
        [_executionProgress] = request =>
        {
            if (request.Kind is NavigationRequestKind.Exit)
            {
                if (_executionProgress.Verification.AssertNotNull().IsChecked)
                {
                    RestartSystem();
                }
                else
                {
                    return (Page?)_askRestartCompleted;
                }
            }
            return null;
        },
        [_askRestartCompleted] = request =>
        {
            if ((request.ClickedButton as Button)?.Text == Buttons.Restart)
            {
                RestartSystem();
            }
            return null;
        }
    }).Show();

    private static async Task<TimeSpan> ExecuteScript(Script script, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new();
        stopwatch.Restart();
        using var execution = script.Execute();
        _ = await execution.WaitForExitAsync(App.Settings.ScriptTimeout, HandleHungScript, cancellationToken); // chaud : what to do with return value
        return stopwatch.Elapsed;

        bool HandleHungScript()
        {
            Logs.HungScript.FormatWith(script.InvariantName, App.Settings.ScriptTimeout).Log(LogLevel.Warning);
            Button endTask = new(Buttons.EndTask);
            using Page page = new()
            {
                IsCancelable = true,
                WindowTitle = PageWindowTitle,
                Icon = DialogIcon.Warning,
                Content = ScriptExecution.HungScriptDialogContent.FormatWith(script.Name, App.Settings.ScriptTimeout),
                Buttons = { endTask, Button.Ignore },
            };
            return new Dialog(page).Show() != endTask;
        }
    }

    private static TimeSpan GetCumulatedExecutionTime(IEnumerable<Script> scripts)
        => scripts.Aggregate(TimeSpan.Zero, (sumSoFar, next) => sumSoFar
           + (App.ScriptExecutionTimes.TryGetValue(next.InvariantName, out TimeSpan t) ? t : App.Settings.ScriptTimeout));

    private static void RestartSystem()
    {
        Logs.RebootingForAppMaintenance.Log();
        Process.Start("shutdown", "/g /t 0 /d p:4:1")?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }
}