using System.Diagnostics;
using System.Globalization;

using Humanizer;
using Humanizer.Localisation;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;

using WinCopies.Linq;

namespace Scover.WinClean.Presentation;

/// <summary>
/// Walks the user through the multi-step high-level operation of executing multiple scripts asynchronously by displaying a task
/// dialog tracking the progress.
/// </summary>
public sealed partial class ScriptExecutionWizard : IDisposable
{
    private readonly ScriptExecutor _executor = new();
    private readonly IReadOnlyList<Script> _scripts;

    /// <param name="scripts">The scripts to execute.</param>
    /// <exception cref="ArgumentException"><paramref name="scripts"/> is empty.</exception>
    public ScriptExecutionWizard(IEnumerable<Script> scripts)
    {
        _scripts = scripts.ToList();
        if (!_scripts.Any())
        {
            throw new ArgumentException("The collection is empty.", nameof(scripts));
        }
        _executor.ProgressChanged += (_, e) => Logs.ScriptExecuting.FormatWith(_scripts[e.ScriptIndex]);
    }

    public void Dispose() => _executor.Dispose();

    /// <summary>Executes the script(s) and displays a dialog tracking the progress.</summary>
    public void Execute()
    {
        bool? createRestorePoint = AskToCreateRestorePoint();
        if (createRestorePoint is null)
        {
            return;
        }
        if (createRestorePoint.Value)
        {
        retry:
            try
            {
                if (!CreateRestorePoint())
                {
                    return;
                }
            }
            catch (InvalidOperationException e)
            {
                Logs.RestorePointCreationError.FormatWith(e.Message).Log(LogLevel.Error);

                bool? enableSystemRestore = AskToEnableSystemRestore();

                if (enableSystemRestore is null)
                {
                    return;
                }

                if (enableSystemRestore.Value)
                {
                    RestorePoint.EnableSystemRestore();
                    Logs.SystemRestoreEnabled.Log(LogLevel.Info);
                    goto retry;
                }
            }
        }

        var (completed, restartQueried) = ShowScriptExecutionProgressDialog();
        if (completed)
        {
            ShowCompletedDialog(restartQueried);
        }
    }

    /// <returns><see langword="true"/> if the restore point was created successfully, otherwise <see langword="false"/>.</returns>
    private static bool CreateRestorePoint()
    {
        Logs.CreatingRestorePoint.Log(LogLevel.Info);

        using Dialogs.ProgressDialog restorePointProgressDialog = new(Button.Stop)
        {
            Content = Resources.UI.Dialogs.CreatingRestorePointContent,
            Style = ProgressBarStyle.MarqueeProgressBar,
            CustomMainIcon = Helpers.GetRestorePointIcon()! // null is ok
        };

        CancellationTokenSource cts = new();
        Exception? exception = null;

        restorePointProgressDialog.Created += async (_, _) =>
        {
            RestorePoint r = new(AppInfo.Name,
                                 EventType.BeginSystemChange,
                                 RestorePointType.ModifySettings);
            await Task.Run(() =>
            {
                try
                {
                    r.Create();
                }
                catch (InvalidOperationException e)
                {
                    exception = e;
                }
            }, cts.Token).ConfigureAwait(true);
            restorePointProgressDialog.Close();
        };

        bool succeded = restorePointProgressDialog.ShowDialog().WasClosed;
        if (exception is not null)
        {
            throw exception;
        }
        if (succeded)
        {
            Logs.RestorePointCreated.Log(LogLevel.Info);
        }
        else
        {
            cts.Cancel();
        }
        return succeded;
    }

    private static void RebootForApplicationMaintenance()
    {
        Logs.RebootingForAppMaintenance.Log(LogLevel.Info);
        _ = Process.Start("shutdown", "/g /t 0 /d p:4:1");
    }

    private async Task ExecuteScriptsAsync()
    {
        Logs.StartingExecutionOfScripts.FormatWith(_scripts.Count).Log(LogLevel.Info);

        await _executor.ExecuteScriptsAsync(_scripts, AskToIgnoreOrKillHungScript).ConfigureAwait(false);

        Logs.ScriptsExecuted.Log(LogLevel.Info);
    }

    private void ShowCompletedDialog(bool restartQueried)
    {
        using Dialog completedDialog = new(Button.Restart, Button.OK)
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

        void UpdateExpandedInfo()
            => progressDialog.ExpandedInformation = ScriptExecutionProgressDialog.ExpandedInformation
               .FormatWith(_scripts[scriptIndex].Name, timeRemaining.Humanize(precision: 3, minUnit: TimeUnit.Second));

        static TimeSpan GetExecutionTime(Script script)
        {
            string? executionTimeString = AppInfo.PersistentSettings.ScriptExecutionTimes[script.InvariantName];
            return executionTimeString is null ? AppInfo.Settings.ScriptTimeout : TimeSpan.ParseExact(executionTimeString, "c", CultureInfo.InvariantCulture);
        }
    }
}