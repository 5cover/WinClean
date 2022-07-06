using Microsoft.WindowsAPICodePack.Win32Native.Shell;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.Logic;
using Scover.WinClean.Operational;
using Scover.WinClean.Presentation.Dialogs;

using System.Runtime.InteropServices;

using static Scover.WinClean.Resources.UI.Buttons;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class ProgressDialog : Dialog
{
    private readonly IReadOnlyList<Script> _scripts;
    private TimeSpan _elapsed;
    private int _scriptIndex;

    public ProgressDialog(IReadOnlyList<Script> scripts)
    {
        _scripts = scripts;

        MinimizeBox = true;
        ExpandedInformation = Resources.UI.ProgressDialog.ExpandedInformation.FormatWith(null, null);
        ExpandedByDefault = App.Settings.DetailsDuringExecution;
        ProgressBarMaximum = _scripts.Count;
        ProgressBarStyle = ProgressBarStyle.ProgressBar;
        Content = Resources.UI.ProgressDialog.Content;
        VerificationText = Resources.UI.ProgressDialog.VerificationText;

        CustomMainIcon = StockIconIdentifier.Software.ToIcon();

        Buttons.Add(new(Cancel));

        VerificationClicked += (_, _) => AutoRestart = IsVerificationChecked;
        ExpandButtonClicked += (_, _) => App.Settings.DetailsDuringExecution ^= true;

        RaiseTimerEvent = true;
        Timer += (_, e) =>
        {
            _elapsed = TimeSpan.FromMilliseconds(e.TickCount);
            UpdateExpandedInfo();
        };
    }

    public bool AutoRestart { get; private set; }

    /// <summary>Gets the number of elapsed seconds since the scripts started executing.</summary>
    public int ElapsedSeconds => Convert.ToInt32(_elapsed.TotalSeconds);

    /// <summary>The index of the last executed script.</summary>
    public int ScriptIndex
    {
        get => _scriptIndex;
        set
        {
            _scriptIndex = value;
            ProgressBarValue = value + 1;
            UpdateExpandedInfo();
        }
    }

    /// <summary>Forcefully destroys this dialog.</summary>
    public void Close()
    {
        if (!DestroyWindow(Handle))
        {
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyWindow(IntPtr hwnd);
    }

    /// <returns>A <see cref="DialogResult"/> value based on the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.Show"/>
    // Not using ShowDialog to avoid destroying the parent window when Close is called (probably because of WM_NOTIFY)
    public new DialogResult Show() => Buttons.IndexOf(base.Show()) switch
    {
        0 => DialogResult.Cancel,
        _ => DialogResult.Closed
    };

    protected override void OnButtonClicked(TaskDialogItemClickedEventArgs e)
    {
        if (e.Item == Buttons[0])
        {
            e.Cancel = YesNoDialog.AbortOperation.ShowDialog() == DialogResult.No;
        }
        base.OnButtonClicked(e);
    }

    private void UpdateExpandedInfo()
                // Not using _elapsed for elapsed time to hide the milliseconds. TimeSpan is used for formatting.
                => ExpandedInformation = Resources.UI.ProgressDialog.ExpandedInformation.FormatWith(_scripts[ScriptIndex].Name, TimeSpan.FromSeconds(ElapsedSeconds));
}