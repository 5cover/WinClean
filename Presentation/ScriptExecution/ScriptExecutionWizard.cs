using System.Diagnostics;

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
public class ScriptExecutionWizard
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
            throw new ArgumentException(DevException.CollectionEmpty, nameof(scripts));
        }
        _executor.ProgressChanged += (_, e) => Logs.ScriptExecuted.FormatWith(_scripts[e.ScriptIndex]);
    }

    /// <summary>Executes the script(s) and displays a dialog tracking the progress.</summary>
    public void Execute()
    {
        Happenings.ScriptExecution.SetAsHappening();

        using WarningDialog warning = new();

        if (warning.ShowDialog() != Button.Continue) return;

        using TaskDialog restorePointDialog = new()
        {
            ButtonStyle = TaskDialogButtonStyle.CommandLinks,
            MainInstruction = SystemRestorePoint.MainInstruction,
            CustomMainIcon = Helpers.GetRestorePointIcon(),
            Buttons =
            {
                new(ButtonType.Cancel),
                new(SystemRestorePoint.CommandLinkYes)
                {
                    Default = true,
                    CommandLinkNote = SystemRestorePoint.CommandLinkYesNote
                },
                new(SystemRestorePoint.CommandLinkNo)
            }
        };
        TaskDialogButton clickedButton = restorePointDialog.ShowDialog();

        if (clickedButton is null || clickedButton == restorePointDialog.Buttons[0]) return;

        if (clickedButton == restorePointDialog.Buttons[1])
        {
            try
            {
                CreateRestorePoint();
            }
            catch (InvalidOperationException e)
            {
                Logs.RestorePointCreationError.FormatWith(e.Message).Log(LogLevel.Error);

                using TaskDialog systemRestoreDisabledDialog = new()
                {
                    ButtonStyle = TaskDialogButtonStyle.CommandLinks,
                    MainInstruction = SystemProtectionDisabled.MainInstruction,
                    MainIcon = TaskDialogIcon.Error,
                    Buttons =
                    {
                        new(ButtonType.Cancel),
                        new(SystemProtectionDisabled.CommandLinkEnable)
                        {
                            CommandLinkNote = SystemProtectionDisabled.CommandLinkEnableNote
                        },
                        new(SystemProtectionDisabled.CommandLinkContinueAnyway)
                        {
                            CommandLinkNote = SystemProtectionDisabled.CommandLinkContinueAnywayNote
                        }
                    }
                };
                TaskDialogButton? result = systemRestoreDisabledDialog.ShowDialog();
                if (result is null || result == systemRestoreDisabledDialog.Buttons[0]) return;
                if (result == systemRestoreDisabledDialog.Buttons[1])
                {
                    RestorePoint.EnableSystemRestore();
                    CreateRestorePoint();
                }
            }
        }

        ShowProgressDialog();

        static void CreateRestorePoint()
        {
            Logs.CreatingRestorePoint.Log();
            new RestorePoint(AppInfo.Name,
                EventType.BeginSystemChange,
                RestorePointType.ModifySettings).Create();
            Logs.RestorePointCreated.Log();
        }
    }

    private static void RebootForApplicationMaintenance()
    {
        Logs.RebootingForAppMaintenance.Log();
        _ = Process.Start("shutdown", "/g /t 0 /d p:4:1");
    }

    private static bool ShowHungScriptDialog(string scriptName)
    {
        using Dialog hungScriptDialog = new(Button.EndTask, Button.Ignore)
        {
            Content = Resources.UI.Dialogs.HungScriptDialogContent,
            Timeout = TimeSpan.FromSeconds(10),
            TimeoutButton = Button.Ignore
        };
        return hungScriptDialog.ShowDialog() == Button.Ignore;
    }

    private async Task ExecuteScriptsAsync()
    {
        Logs.StartingExecutionOfScripts.FormatWith(_scripts.Count).Log(LogLevel.Info);

        await _executor.ExecuteScriptsAsync(_scripts, ShowHungScriptDialog).ConfigureAwait(false);

        Logs.ScriptsExecuted.Log(LogLevel.Info);
    }

    private async void ProgressDialogCreated(object? sender, EventArgs e)
    {
        using ProgressDialog progress = (ProgressDialog)sender.AssertNotNull();
        await ExecuteScriptsAsync().ConfigureAwait(true);
        progress.Close();

        using CompletedDialog completed = new(_scripts.Count, TimeSpan.FromSeconds(progress.ElapsedSeconds));

        if (progress.AutoRestart || completed.ShowDialog() == Button.Restart)
        {
            RebootForApplicationMaintenance();
        }
    }

    private void ShowProgressDialog()
    {
        using ProgressDialog progress = new(_scripts);

        _executor.ProgressChanged += (_, e) => progress.ScriptIndex = e.ScriptIndex;

        progress.Created += ProgressDialogCreated;

        if (progress.Show() != Button.Stop) return;
        _executor.CancelScriptExecution();
        Logs.ScriptExecutionCanceled.Log();
    }
}