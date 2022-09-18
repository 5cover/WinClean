using System.Diagnostics;
using System.Runtime.InteropServices;

using Humanizer;
using Humanizer.Localisation;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;

namespace Scover.WinClean.Presentation.ScriptExecution;

/// <summary>
/// Walks the user through the multi-step high-level operation of executing multiple scripts asynchronously by displaying a task
/// dialog tracking the progress.
/// </summary>
public sealed class ScriptExecutionWizard
{
    private static readonly ScriptExecutor executor = new();
    private readonly IReadOnlyList<Script> _scripts;

    /// <param name="scripts">The scripts to execute.</param>
    /// <exception cref="ArgumentException"><paramref name="scripts"/> is empty.</exception>
    public ScriptExecutionWizard(IEnumerable<Script> scripts)
    {
        _scripts = scripts.ToList();
        if (!_scripts.Any())
        {
            throw new ArgumentException(DevException.CollectionEmpty, nameof(scripts));
        }
        executor.ProgressChanged += (_, e) => Logs.ScriptExecuting.FormatWith(_scripts[e.ScriptIndex]);
    }

    /// <summary>Executes the script(s) and displays a dialog tracking the progress.</summary>
    public void Execute()
    {
        CommandLink yes = new()
        {
            Text = SystemRestorePoint.CommandLinkYes,
            Note = SystemRestorePoint.CommandLinkYesNote
        };

        using CommandLinkDialog restorePointDialog = new()
        {
            MainInstruction = SystemRestorePoint.MainInstruction,
            CustomMainIcon = Helpers.GetRestorePointIcon(),
            CommandLinks =
            {
                yes,
                new()
                {
                    Text = SystemRestorePoint.CommandLinkNo
                }
            },
            DefaultCommandLink = yes,
            AreHyperlinksEnabled = true,
        };
        DialogResult restorePointDialogResult = restorePointDialog.ShowDialog();

        if (restorePointDialogResult.WasClosed)
        {
            return;
        }

        if (ReferenceEquals(restorePointDialogResult.ClickedCommandLink, yes))
        {
            try
            {
                CreateRestorePoint();
            }
            catch (InvalidOperationException e)
            {
                Logs.RestorePointCreationError.FormatWith(e.Message).Log(LogLevel.Error);

                bool? enableSystemRestore = ShowEnableSystemRestoreDialog();

                if (enableSystemRestore is null)
                {
                    return;
                }

                if (enableSystemRestore.Value)
                {
                    RestorePoint.EnableSystemRestore();
                    Logs.SystemRestoreEnabled.Log(LogLevel.Info);
                    CreateRestorePoint();
                }
            }
        }

        (bool completed, bool restartQueried) = ShowScriptExecutionProgressDialog();
        if (completed)
        {
            ShowCompletedDialog(restartQueried);
        }
    }

    private static void CreateRestorePoint()
    {
        Logs.CreatingRestorePoint.Log(LogLevel.Info);

        using Dialogs.ProgressDialog creatingRestorePointProgressDialog = new(Button.Stop)
        {
            Content = Resources.UI.Dialogs.CreatingRestorePointContent,
            Style = ProgressBarStyle.MarqueeProgressBar,
            CustomMainIcon = Helpers.GetRestorePointIcon(),
            ShowMinimizeBox = true,
        };

        Exception? systemProtectionDisabledException = null;

        creatingRestorePointProgressDialog.Created += (_, _) =>
        {
            RestorePoint r = new(AppInfo.Name,
                                 EventType.BeginSystemChange,
                                 RestorePointType.ModifySettings);
            try
            {
                r.Create();
            }
            catch (InvalidOperationException e)
            {
                // Save the thrown exception because the thrown SEHException does not track InnerException.
                systemProtectionDisabledException = e;
                throw;
            }
            finally
            {
                creatingRestorePointProgressDialog.Close();
            }
        };

        try
        {
            creatingRestorePointProgressDialog.ShowDialog();
        }
        catch (SEHException) when (systemProtectionDisabledException is not null)
        {
            throw systemProtectionDisabledException;
        }

        Logs.RestorePointCreated.Log(LogLevel.Info);
    }

    private static void RebootForApplicationMaintenance()
    {
        Logs.RebootingForAppMaintenance.Log(LogLevel.Info);
        _ = Process.Start("shutdown", "/g /t 0 /d p:4:1");
    }

    private static bool? ShowEnableSystemRestoreDialog()
    {
        CommandLink enable = new()
        {
            Text = SystemProtectionDisabled.CommandLinkEnable,
            Note = SystemProtectionDisabled.CommandLinkEnableNote
        };

        using CommandLinkDialog enableSystemRestore = new()
        {
            MainInstruction = SystemProtectionDisabled.MainInstruction,
            MainIcon = TaskDialogIcon.Error,
            CommandLinks =
            {
                enable,
                new()
                {
                    Text = SystemProtectionDisabled.CommandLinkContinueAnyway,
                    Note = SystemProtectionDisabled.CommandLinkContinueAnywayNote
                }
            }
        };
        DialogResult result = enableSystemRestore.ShowDialog();

        // null if the user canceled or closed the dialog; true if user chose to enable system restore; false if user chose to
        // continue anyway.
        return result.WasClosed ? null : ReferenceEquals(result.ClickedCommandLink, enable);
    }

    private static bool ShowHungScriptDialog(string scriptName)
    {
        using TimeoutDialog hungScriptDialog = new(Button.EndTask, Button.Ignore)
        {
            MainIcon = TaskDialogIcon.Warning,
            Content = Resources.UI.Dialogs.HungScriptDialogContent.FormatWith(scriptName, AppInfo.Settings.ScriptTimeout.Humanize(3, minUnit: TimeUnit.Second)),
            Timeout = 10.Seconds(),
            TimeoutButton = Button.Ignore
        };
        return hungScriptDialog.ShowDialog().ClickedButton != Button.EndTask;
    }

    private async Task ExecuteScriptsAsync()
    {
        Logs.StartingExecutionOfScripts.FormatWith(_scripts.Count).Log(LogLevel.Info);

        await executor.ExecuteScriptsAsync(_scripts, ShowHungScriptDialog).ConfigureAwait(false);

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
            // The ApplicationExit event handler will be called before the system reboots, so the scripts and settings will be
            // properly saved.
            RebootForApplicationMaintenance();
        }
    }

    private (bool completed, bool restartQueried) ShowScriptExecutionProgressDialog()
    {
        bool restartQueried = false;
        int scriptIndex = 0;
        TimeSpan timeRemaining = _scripts.Sum(s => s.ExecutionTime);

        using Dialogs.ProgressDialog scriptExecutionProgressDialog = new(Button.Stop)
        {
            Content = ScriptExecutionProgressDialog.Content,
            // Needs to have a value to show expander.
            ExpandedInformation = ScriptExecutionProgressDialog.ExpandedInformation,
            Maximum = _scripts.Count,
            ShowMinimizeBox = true,
            StartExpanded = AppInfo.Settings.DetailsDuringExecution,
            Style = ProgressBarStyle.ProgressBar,
            VerificationText = ScriptExecutionProgressDialog.VerificationText
        };

        scriptExecutionProgressDialog.ExpandButtonClicked += (_, _) => AppInfo.Settings.DetailsDuringExecution ^= true;
        scriptExecutionProgressDialog.VerificationClicked += (_, _) => restartQueried = scriptExecutionProgressDialog.IsVerificationChecked;

        scriptExecutionProgressDialog.Timer += (_, e) =>
        {
            timeRemaining -= e.Elapsed;
            UpdateExpandedInfo();
        };

        executor.ProgressChanged += (_, e) =>
        {
            scriptIndex = e.ScriptIndex;

            scriptExecutionProgressDialog.Value = scriptIndex;
            timeRemaining = _scripts.TakeLast(_scripts.Count - scriptIndex).Sum(s => s.ExecutionTime);
            UpdateExpandedInfo();
        };

        void UpdateExpandedInfo()
            => scriptExecutionProgressDialog.ExpandedInformation = ScriptExecutionProgressDialog.ExpandedInformation
                             .FormatWith(_scripts[scriptIndex].Name,
                                         timeRemaining.Humanize(precision: 3, minUnit: TimeUnit.Second));

        scriptExecutionProgressDialog.Created += async (_, _) =>
        {
            await ExecuteScriptsAsync().ConfigureAwait(true);
            scriptExecutionProgressDialog.Close();
        };
        bool completed = scriptExecutionProgressDialog.Show().ClickedButton != Button.Stop;
        if (!completed)
        {
            executor.CancelScriptExecution();
            Logs.ScriptExecutionCanceled.Log(LogLevel.Info);
        }
        return (completed, restartQueried);
    }
}