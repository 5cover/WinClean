using Scover.Dialogs;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Humanizer;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

/// <summary>
/// Walks the user through the multi-step high-level operation of executing multiple scripts asynchronously by displaying a
/// dialog tracking the progress.
/// </summary>
public sealed partial class ScriptExecutionWizard
{
    private readonly Page _askRestartCompleted;
    private readonly Page _askRestorePoint;
    private readonly MultiPageDialog _dialog;
    private readonly Page _executionProgress;
    private readonly Dictionary<Page, NextPageSelector> _nextPageSelectors = new();
    private readonly Page _restorePointProgress;

    private readonly IReadOnlyList<Script> _scripts;

    /// <summary>Executes the script(s) and displays a dialog tracking the progress.</summary>
    public void Execute() => _dialog.Show();

    private static TimeSpan? GetExecutionTime(Script script)
        => App.ScriptExecutionTimes.TryGetValue(script.InvariantName, out TimeSpan executionTime) ? executionTime : null;

    private static void RestartSystem()
    {
        Logs.RebootingForAppMaintenance.Log();
        Process.Start("shutdown", "/g /t 0 /d p:4:1")?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    private async IAsyncEnumerable<TimeSpan> ExecuteScripts([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Logs.StartingExecution.FormatWith(_scripts.Count).Log(LogLevel.Info);
        Stopwatch stopwatch = new();

        foreach (Script script in _scripts)
        {
            stopwatch.Restart();
            await script.ExecuteAsync(cancellationToken);
            yield return stopwatch.Elapsed;
        }
        Logs.ScriptsExecuted.Log(LogLevel.Info);
    }

    private void ShowCompletedDialog(bool restartQueried)
    {
        using Dialog completedDialog = new(Button.Restart, Button.Ok)
        {
            ExpandedInformation = CompletedDialog.ExpandedInformation.FormatWith(_scripts.Count),
            StartExpanded = AppInfo.Settings.DetailsAfterExecution,
            MainInstruction = CompletedDialog.MainInstruction,
            Content = CompletedDialog.Content,
        };
        completedDialog.ExpandButtonClicked += (_, _) => AppInfo.Settings.DetailsAfterExecution ^= true;

        if (restartQueried || completedDialog.ShowDialog().ClickedButton == Button.Restart)
        {
            RebootForApplicationMaintenance();
        }
    }

    private (bool completed, bool restartQueried) ShowScriptExecutionProgressDialog()
    {
        bool restartQueried = false;
        int scriptIndex = 0;
        TimeSpan timeRemaining = _scripts.Sum(GetExecutionTime);

        using Dialogs.ProgressDialog progressDialog = new(Button.Stop)
        {
            Content = ScriptExecutionProgressDialog.Content,
            // ExpandedInformation needs to start with a non-empty string for the expander control to be visible.
            ExpandedInformation = " ",
            Maximum = _scripts.Count,
            ShowMinimizeBox = true,
            StartExpanded = AppInfo.Settings.DetailsDuringExecution,
            Style = ProgressBarStyle.ProgressBar,
            VerificationText = ScriptExecutionProgressDialog.VerificationText
        };

        progressDialog.ExpandButtonClicked += (_, _) => AppInfo.Settings.DetailsDuringExecution ^= true;
        progressDialog.VerificationClicked += (_, _) => restartQueried = progressDialog.IsVerificationChecked;

        progressDialog.Timer += (_, e) =>
        {
            timeRemaining -= e.Elapsed;
            UpdateExpandedInfo();
        };

        _executor.ProgressChanged += (_, e) =>
        {
            progressDialog.Value = e.ScriptIndex;
            timeRemaining -= GetExecutionTime(_scripts[e.ScriptIndex]);
            if (timeRemaining < TimeSpan.Zero)
            {
                timeRemaining = TimeSpan.Zero;
            }
            UpdateExpandedInfo();
        };

        progressDialog.Created += async (_, _) =>
        {
            await ExecuteScriptsAsync().ConfigureAwait(true);
            progressDialog.Close();
        };

        bool completed = progressDialog.ShowDialog().ClickedButton != Button.Stop;
        if (!completed)
        {
            _executor.CancelScriptExecution();
            Logs.ScriptExecutionCanceled.Log(LogLevel.Info);
        }
        return (completed, restartQueried);

        void UpdateExpandedInfo() => progressDialog.ExpandedInformation = ScriptExecutionProgressDialog.ExpandedInformation
            .FormatWith(_scripts[scriptIndex].Name, timeRemaining.Humanize(precision: 3, minUnit: TimeUnit.Second));

        static TimeSpan GetExecutionTime(Script script)
        {
            string? executionTimeString = AppInfo.PersistentSettings.ScriptExecutionTimes[script.InvariantName];
            return executionTimeString is null ? AppInfo.Settings.ScriptTimeout : TimeSpan.ParseExact(executionTimeString, "c", CultureInfo.InvariantCulture);
        }
    }
}