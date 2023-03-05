using System.Diagnostics;

using Humanizer.Localisation;

using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;

using static Scover.WinClean.Resources.UI.ScriptExecution;

namespace Scover.WinClean.Presentation;

public partial class ScriptExecutionWizard
{
    /// <param name="scripts">The scripts to execute.</param>
    /// <exception cref="ArgumentException"><paramref name="scripts"/> is empty.</exception>
    public ScriptExecutionWizard(IReadOnlyList<Script> scripts)
    {
        if (!scripts.Any())
        {
            throw new ArgumentException("The list is empty.", nameof(scripts));
        }

        _scripts = scripts;

        #region Ask to create restore point
        {
            Button linkYes = new(AskRestorePoint.CommandLinkYes, AskRestorePoint.CommandLinkYesNote);
            _askRestorePoint = new()
            {
                AllowHyperlinks = true,
                WindowTitle = PageWindowTitle,
                Icon = restorePointIcon,
                MainInstruction = AskRestorePoint.MainInstruction,
                Buttons = new(ButtonStyle.CommandLinks, linkYes)
                {
                    linkYes,
                    AskRestorePoint.CommandLinkNo,
                    Button.Cancel,
                },
            };
        }
        #endregion Ask to create restore point

        #region Restore point creation progress
        {
            _restorePointProgress = new()
            {
                WindowTitle = PageWindowTitle,
                Icon = restorePointIcon,
                MainInstruction = CreatingRestorePointContent,
                ProgressBar = new()
                {
                    Mode = ProgressBarMode.Marquee
                },
                Buttons = { Buttons.Stop },
            };
            _restorePointProgress.Created += CreateRestorePoint;
        }
        #endregion Restore point creation progress

        #region Ask restart on completed
        {
            _askRestartCompleted = new()
            {
                IsCancelable = true,
                WindowTitle = PageWindowTitle,
                Header = DialogHeader.Green,
                Icon = DialogIcon.SuccessShield,
                MainInstruction = ExecutionCompletedMainInstruction,
                Content = ExecutionCompletedContent.FormatWith(_scripts.Count),
                Buttons = { Buttons.Restart, Button.Close },
            };
        }
        #endregion Ask restart on completed

        #region Execution progress
        {
            Button stop = new(Buttons.Stop);
            stop.Clicked += (s, e) =>
            {
                using Page page = DialogPageFactory.ConfirmAbortOperation();
                e.Cancel = Button.No.Equals(new Dialog(page).Show());
            };

            _executionProgress = new()
            {
                WindowTitle = PageWindowTitle,
                Icon = softwareIcon,
                MainInstruction = ExecutionProgressMainInstruction,
                Content = ExecutionProgressContent,
                ProgressBar = new()
                {
                    Maximum = _scripts.Count
                },
                Expander = new()
                {
                    IsExpanded = App.Settings.ExecutionShowDetails
                },
                Verification = new(ExecutionProgressVerificationText),
                Buttons = { stop },
            };
            _executionProgress.Expander.ExpandedChanged += (_, _) => App.Settings.ExecutionShowDetails ^= true;
            _executionProgress.Created += StartExecution;
            _executionProgress.Created += StartTimer;
        }
        #endregion Execution progress
    }

    private static string PageWindowTitle => WindowTitle.FormatWith(AppMetadata.Name);

    private async void CreateRestorePoint(object? sender, EventArgs args)
    {
        await Task.Run(() =>
        {
            // Some drives are non-eligible for system restore, but Enable-ComputerRestore will still enable
            // the eligible ones.
            using Process powerShell = $"-Command Enable-ComputerRestore -Drive {string.Join(',', DriveInfo.GetDrives().Select(di => @$"""{di.Name}\"""))}".StartPowerShellWithArguments().AssertNotNull();
            powerShell.WaitForExit();
            new RestorePoint(AppMetadata.Name, EventType.BeginSystemChange, RestorePointType.ModifySettings).Create();
        });
        _restorePointProgress.Exit();
        Logs.RestorePointCreated.Log();
    }

    private async void StartTimer(object? s, EventArgs e)
    {
        while (await _timer.WaitForNextTickAsync())
        {
            if (_timeRemaining < _timerInterval)
            {
                _timeRemaining = TimeSpan.Zero;
            }
            else
            {
                _timeRemaining -= _timerInterval;
            }
            UpdateExpandedInfo();
        }
    }

    private async void StartExecution(object? s, EventArgs e)
    {
        Logs.StartingExecution.FormatWith(_scripts.Count).Log(LogLevel.Info);
        UpdateExpandedInfo();
        foreach (var script in _scripts)
        {
            _timeRemaining = GetCumulatedExecutionTime(_scripts.Skip(_scriptIndex));
            UpdateExpandedInfo();
            try
            {
                App.ScriptExecutionTimes[script.InvariantName] = await ExecuteScript(script, _ctsScriptExecution.Token);
            }
            catch (OperationCanceledException)
            {
                // Stop execution
                return;
            }
            catch (TimeoutException)
            {
                // The script was hung and the user terminated it, skip to the next one.
            }
            _executionProgress.ProgressBar.AssertNotNull().Value = ++_scriptIndex;
            Logs.ScriptExecuted.FormatWith(script.InvariantName).Log();
        }
        Logs.ScriptsExecuted.Log(LogLevel.Info);
        _executionProgress.Exit();
        _timer.Dispose();
    }

    private void UpdateExpandedInfo() => _executionProgress.Expander.AssertNotNull().Text = ExecutionProgressExpanderText
        .FormatWith(_scriptIndex,
                    _scripts.Count,
                    _scripts.Count - _scriptIndex,
                    _scripts[Math.Min(_scripts.Count - 1, _scriptIndex)].Name,
                    _timeRemaining.Humanize(precision: 3, minUnit: TimeUnit.Second));
}