using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.ScriptExecution;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Resources;

using System.Diagnostics;

namespace Scover.WinClean.Presentation.ScriptExecution;

/// <summary>
/// Walks the user through the multistep high-level operation executing multiple scripts asynchronously by displaying a task
/// dialog tracking the progress.
/// </summary>
public class ScriptExecutionWizard
{
    private readonly ScriptExecutor _executor = new();
    private readonly IReadOnlyList<Script> _scripts;

    /// <param name="scripts">The scripts to execute.</param>
    /// <exception cref="InvalidOperationException"><paramref name="scripts"/> is empty.</exception>
    public ScriptExecutionWizard(IEnumerable<Script> scripts)
    {
        if (!scripts.Any())
        {
            throw new InvalidOperationException(DevException.CollectionEmpty);
        }
        _scripts = scripts.ToList();
        _executor.ProgressChanged += (sender, e) => Logs.ScriptExecuted.FormatWith(_scripts[e.ScriptIndex]);
    }

    /// <param name="script">The script to execute.</param>
    public ScriptExecutionWizard(Script script) : this(new List<Script> { script })
    {
    }

    /// <summary>Executes the script(s) without displaying a dialog.</summary>
    public void ExecuteNoUI() => ExecuteScriptsAsync().ConfigureAwait(false);

    /// <summary>Executes the script(s) and displays a dialog tracking the progress.</summary>
    public void ExecuteUI()
    {
        Happenings.ScriptExecution.SetAsHappening();

        using WarningDialog warning = new();

        if (warning.ShowDialog() == DialogResult.Continue)
        {
            if (YesNoDialog.SystemRestorePoint.ShowDialog() == DialogResult.Yes)
            {
                bool retry;
                do
                {
                    try
                    {
                        CreateRestorePoint();
                        retry = false;
                    }
                    catch (SystemProtectionDisabledException e)
                    {
                        Logs.RestorePointCreationError.FormatWith(e.Message).Log(LogLevel.Error);

                        DialogResult result = RetryIgnoreAbort.SystemRestoreDisabled.ShowDialog();
                        if (result == DialogResult.Abort)
                        {
                            return;
                        }
                        retry = result == DialogResult.Retry;
                    }
                } while (retry);
            }

            ShowProgressDialog();

            static void CreateRestorePoint()
            {
                Logs.CreatingRestorePoint.Log();
                new RestorePoint(App.Name ?? string.Empty,
                                 EventType.BeginSystemChange,
                                 RestorePointType.ModifySettings).Create();
                Logs.RestorePointCreated.Log();
            }
        }
    }

    private static void RebootForApplicationMaintenance()
    {
        Logs.RebootingForAppMaintenance.Log();
        _ = Process.Start("shutdown", $"/g /t 0 /d p:4:1");
    }

    private async Task ExecuteScriptsAsync()
    {
        Logs.StartingExecutionOfScripts.FormatWith(_scripts.Count).Log(LogLevel.Info);

        await _executor.ExecuteScriptsAsync(_scripts,
                                            App.Settings.ScriptTimeout,
                                            name => new CustomDialog(Resources.UI.Buttons.Ignore, Resources.UI.Buttons.EndTask)
                                            {
                                                MainIcon = TaskDialogIcon.Warning,
                                                Content = Resources.UI.ScriptExecutionWizard.HungScriptDialogContent.FormatWith(name, App.Settings.ScriptTimeout)
                                            }.ShowDialog() == Resources.UI.Buttons.EndTask,
                                            (e, fSInfo, verb) => FSErrorFactory.MakeFSError<RetryExitDialog>(e, verb, fSInfo).ShowDialog() == DialogResult.Retry).ConfigureAwait(false);

        Logs.ScriptsExecuted.Log(LogLevel.Info);
    }

    private async void ProgressDialog_Created(object? sender, EventArgs e)
    {
        using ProgressDialog progress = (ProgressDialog)sender!; // ! : the dialog cannot be null, it was just created
        await ExecuteScriptsAsync().ConfigureAwait(true);
        progress.Close();

        using CompletedDialog completed = new(_scripts.Count, TimeSpan.FromSeconds(progress.ElapsedSeconds));

        if (progress.AutoRestart || completed.ShowDialog() == DialogResult.Restart)
        {
            RebootForApplicationMaintenance();
        }
    }

    private void ShowProgressDialog()
    {
        using ProgressDialog progress = new(_scripts);

        _executor.ProgressChanged += (sender, e) => progress.ScriptIndex = e.ScriptIndex;

        progress.Created += ProgressDialog_Created;

        if (progress.Show() is DialogResult.Cancel or DialogResult.Closed)
        {
            _executor.CancelScriptExecution();
            Logs.ScriptExecutionCanceled.Log();
        }
    }
}