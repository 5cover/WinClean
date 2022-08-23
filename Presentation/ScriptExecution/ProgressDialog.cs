using System.Globalization;

using Humanizer;
using Humanizer.Localisation;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.Presentation.Dialogs;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class ProgressDialog : Dialog
{
    private readonly IReadOnlyList<Script> _scripts;
    private double _estimatedRemainingMilliseconds;
    private int _scriptIndex;

    public ProgressDialog(IReadOnlyList<Script> scripts) : base(Button.Stop)
    {
        _scripts = scripts;

        AllowDialogCancellation = false;
        ShowMinimizeBox = true;
        ExpandedInformation = Resources.UI.ProgressDialog.ExpandedInformation.FormatWith(CultureInfo.CurrentCulture, null, null);
        StartExpanded = AppInfo.Settings.DetailsDuringExecution;
        Dlg.ProgressBarMaximum = scripts.Count;
        Dlg.ProgressBarStyle = ProgressBarStyle.ProgressBar;
        Content = Resources.UI.ProgressDialog.Content;
        VerificationText = Resources.UI.ProgressDialog.VerificationText;

        VerificationClicked += (_, _) => RestartQueried = IsVerificationChecked;
        ExpandButtonClicked += (_, _) => AppInfo.Settings.DetailsDuringExecution ^= true;

        SetConfirmation(Button.Stop, () => new Dialog(Button.Yes, Button.No)
        {
            MainIcon = TaskDialogIcon.Warning,
            Content = Resources.UI.Dialogs.ConfirmAbortOperationContent,
            DefaultButton = Button.No
        }.ShowDialog().ClickedButton == Button.Yes);

        Dlg.Timer += (_, e) =>
        {
            // Here ticks are actually milliseconds
            _estimatedRemainingMilliseconds -= e.TickCount;
            UpdateExpandedInfo();
            e.ResetTickCount = true;
        };
        Dlg.RaiseTimerEvent = true;
    }

    public bool RestartQueried { get; private set; }

    /// <summary>The index of the currently executing script.</summary>
    public int ScriptIndex
    {
        get => _scriptIndex;
        set
        {
            _scriptIndex = value;
            Dlg.ProgressBarValue = value;
            _estimatedRemainingMilliseconds = _scripts.TakeLast(_scripts.Count - _scriptIndex).Select(s => s.ExecutionTime).Sum(t => t.TotalMilliseconds);
            UpdateExpandedInfo();
        }
    }

    private void UpdateExpandedInfo()
        => ExpandedInformation = Resources.UI.ProgressDialog.ExpandedInformation
                                  .FormatWith(_scripts[ScriptIndex].Name,
                                              // Seconds precision
                                              TimeSpan.FromMilliseconds(_estimatedRemainingMilliseconds).Humanize(precision: 3, minUnit: TimeUnit.Second));
}