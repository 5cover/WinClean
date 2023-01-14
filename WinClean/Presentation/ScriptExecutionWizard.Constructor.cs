using System.Diagnostics;
using Humanizer;
using Humanizer.Localisation;
using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using Scover.WinClean.Resources.UI;
using Vanara.PInvoke;
using static Scover.WinClean.Resources.UI.Dialogs;

namespace Scover.WinClean.Presentation;

public partial class ScriptExecutionWizard
{
    /// <param name="scripts">The scripts to execute.</param>
    /// <exception cref="ArgumentException"><paramref name="scripts"/> is empty.</exception>
    public ScriptExecutionWizard(IReadOnlyList<Script> scripts)
    {
        ushort piIcon = 0;
        var restorePointIconHandle = Shell32.ExtractAssociatedIcon(IntPtr.Zero, new(Path.Join(Environment.SystemDirectory, "rstrui.exe")), ref piIcon);
        var restorePointIcon = DialogIcon.FromHandle(restorePointIconHandle.DangerousGetHandle());

        if (!scripts.Any())
        {
            throw new ArgumentException("The list is empty.", nameof(scripts));
        }

        _scripts = scripts;

        #region Ask to create restore point
        {
            CommandLink linkYes = new(AskRestorePoint.CommandLinkYes, AskRestorePoint.CommandLinkYesNote);
            _askRestorePoint = new Page()
            {
                AllowHyperlinks = true,
                Buttons = new(defaultItem: linkYes, style: CommitControlStyle.CommandLinks)
                {
                    linkYes,
                    AskRestorePoint.CommandLinkNo,
                },
                MainInstruction = AskRestorePoint.MainInstruction,
                Icon = restorePointIcon,
                IsCancelable = true,
            };
            _nextPageSelectors.Add(_askRestorePoint, clicked =>
            {
                var txt = (clicked as CommandLink)?.Label;
                return txt == AskRestorePoint.CommandLinkYes
                     ? _restorePointProgress
                     : txt == AskRestorePoint.CommandLinkNo
                     ? _executionProgress
                     : null;
            });
        }
        #endregion Ask to create restore point

        #region Restore point creation progress
        {
            _restorePointProgress = new()
            {
                Buttons = { Buttons.Stop },
                Icon = restorePointIcon,
                MainInstruction = CreatingRestorePointContent,
                ProgressBar = new()
                {
                    Mode = ProgressBarMode.Marquee
                },
            };
            _nextPageSelectors.Add(_restorePointProgress, clicked => (clicked as Button)?.Text == Buttons.Stop ? null : _executionProgress);
            _restorePointProgress.Created += async (_, _) =>
            {
                await Task.Run(() =>
                {
                    // Some drives are non-eligible for system restore, but Enable-ComputerRestore will still enable the
                    // eligible ones.
                    using Process powerShell = $"-Command Enable-ComputerRestore -Drive {string.Join(',', DriveInfo.GetDrives().Select(di => @$"""{di.Name}\"""))}".StartPowerShellWithArguments().AssertNotNull();
                    powerShell.WaitForExit();
                    new RestorePoint(AppMetadata.Name, EventType.BeginSystemChange, RestorePointType.ModifySettings).Create();
                });
                _restorePointProgress.Close();
                Logs.RestorePointCreated.Log();
            };
        }
        #endregion Restore point creation progress

        #region Ask restart on completed
        {
            _askRestartCompleted = new()
            {
                Buttons = { Buttons.Restart, Button.Close },
                Content = ExecutionCompletedContent.FormatWith(_scripts.Count),
                Icon = DialogIcon.SuccessShield,
                Header = DialogHeader.Green,
                MainInstruction = ExecutionCompletedMainInstruction
            };
            _nextPageSelectors.Add(_askRestartCompleted, clicked =>
            {
                if (_executionProgress?.Verification?.IsChecked ?? false || (clicked as Button)?.Text == Buttons.Restart)
                {
                    RestartSystem();
                }
                return null;
            });
        }
        #endregion Ask restart on completed

        #region Execution progress
        {
            _executionProgress = new()
            {
                IsMinimizable = true,
                IsCancelable = false,
                Buttons = { Buttons.Stop },
                Content = ExecutionProgressContent,
                Expander = new()
                {
                    Text = ExecutionProgressExpanderTextInitializing,
                    IsExpanded = App.Settings.ExecutionShowDetails
                },
                MainInstruction = ExecutionProgressMainInstruction,
                ProgressBar = new()
                {
                    Maximum = _scripts.Count
                },
                Verification = new(ExecutionProgressVerificationText)
            };
            _nextPageSelectors.Add(_executionProgress, clicked => (clicked as Button)?.Text == Buttons.Stop ? null : _askRestartCompleted);
            _executionProgress.Expander.ExpandedChanged += (_, _) => App.Settings.ExecutionShowDetails ^= true;
            CancellationTokenSource cts = new();
            TimeSpan? timeRemaining = GetCumulatedExecutionTime(_scripts);
            int scriptIndex = 0;
            TimeSpan timerInterval = 1.Seconds();
            using PeriodicTimer timer = new(timerInterval);

            _executionProgress.Created += StartExecution;
            _executionProgress.Created += StartTimer;
            _executionProgress.Destroyed += (s, e) => cts.Cancel();

            async void StartTimer(object? s, EventArgs e)
            {
                while (await timer.WaitForNextTickAsync(cts.Token))
                {
                    if (timeRemaining is TimeSpan tr && tr >= timerInterval)
                    {
                        timeRemaining = tr - timerInterval;
                    }
                    UpdateExpandedInfo();
                }
            }

            async void StartExecution(object? s, EventArgs e)
            {
                await foreach (TimeSpan executionTime in ExecuteScripts(cts.Token))
                {
                    App.ScriptExecutionTimes[_scripts[scriptIndex].InvariantName] = executionTime;
                    Logs.ScriptExecuted.FormatWith(_scripts[scriptIndex].InvariantName).Log();
                    UpdateExpandedInfo();
                    ++scriptIndex;
                    timeRemaining = GetCumulatedExecutionTime(_scripts.Skip(scriptIndex));
                    _executionProgress.ProgressBar.Value = scriptIndex;
                }

                _executionProgress.Close();
            }

            void UpdateExpandedInfo() => _executionProgress.Expander.Text = ExecutionProgressExpanderText
                .FormatWith(scriptIndex,
                            _scripts.Count,
                            _scripts.Count - scriptIndex,
                            _scripts[scriptIndex].Name,
                            timeRemaining?.Humanize(precision: 3, minUnit: TimeUnit.Second) ?? ExecutionProgressTimeRemainingUnknown);

            TimeSpan? GetCumulatedExecutionTime(IEnumerable<Script> scripts)
                => scripts.Aggregate<Script, TimeSpan?>(null, (sumSoFar, next) => sumSoFar + GetExecutionTime(next));
        }
        #endregion Execution progress

        _dialog = new(_askRestorePoint, _nextPageSelectors);
    }
}