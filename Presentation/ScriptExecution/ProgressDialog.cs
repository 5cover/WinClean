using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class ProgressDialog : Dialog
{
    private readonly IReadOnlyList<Script> _scripts;
    private TimeSpan _elapsed;
    private int _scriptIndex;

    public ProgressDialog(IReadOnlyList<Script> scripts) : base(Button.Stop)
    {
        _scripts = scripts;

        AllowDialogCancellation = false;
        ShowMinimizeBox = true;
        ExpandedInformation = Resources.UI.ProgressDialog.ExpandedInformation.FormatWith(null, null);
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

        Dlg.RaiseTimerEvent = true;
        Dlg.Timer += (_, e) =>
        {
            _elapsed = TimeSpan.FromMilliseconds(e.TickCount);
            UpdateExpandedInfo();
        };
    }

    /// <summary>Gets the number of elapsed seconds since the scripts started executing.</summary>
    public int ElapsedSeconds => Convert.ToInt32(_elapsed.TotalSeconds);

    public bool RestartQueried { get; private set; }

    /// <summary>The index of the currently executing script.</summary>
    public int ScriptIndex
    {
        get => _scriptIndex;
        set
        {
            _scriptIndex = value;
            Dlg.ProgressBarValue = value;
            UpdateExpandedInfo();
        }
    }

    private void UpdateExpandedInfo()
                // Not using elapsed for elapsed time to hide the milliseconds. TimeSpan is used for formatting.
                => ExpandedInformation = Resources.UI.ProgressDialog.ExpandedInformation.FormatWith(_scripts[ScriptIndex].Name, TimeSpan.FromSeconds(ElapsedSeconds));
}