using Ookii.Dialogs.Wpf;

using static Scover.WinClean.Resources.UI.Buttons;

namespace Scover.WinClean.Presentation.Dialogs;

public class YesNoDialog : Dialog
{
    protected YesNoDialog()
    {
        Buttons.Add(new(Yes));
        Buttons.Add(new(No));
    }

    public static YesNoDialog AbortOperation => new()
    {
        MainIcon = TaskDialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmAbortOperationContent
    };

    public static YesNoDialog ScriptDeletion => new()
    {
        MainIcon = TaskDialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmScriptDeletionContent
    };

    public static YesNoDialog SystemRestorePoint => new()
    {
        MainIcon = TaskDialogIcon.Warning,
        Content = Resources.UI.Dialogs.SystemRestorePointContent
    };

    /// <returns>A <see cref="DialogResult"/> value based on the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new DialogResult ShowDialog() => Buttons.IndexOf(base.ShowDialog()) switch
    {
        0 => DialogResult.Yes,
        1 => DialogResult.No,
        _ => DialogResult.Closed
    };
}