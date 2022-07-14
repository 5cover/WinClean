using Ookii.Dialogs.Wpf;

using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources.UI;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>
/// Implements factory methods for commonly used dialogs. Helps making the UI feel more consistent by centralizing dialog creation.
/// </summary>
public static class DialogFactory
{
    public static Dialog MakeInvalidScriptDataDialog(Exception e, string path, params Button[] buttons)
    {
        Dialog dialog = new(buttons)
        {
            MainIcon = TaskDialogIcon.Error,
            MainInstruction = InvalidScript.MainInstruction,
            Content = InvalidScript.Content.FormatWith(Path.GetFileName(path)),
            EnableHyperlinks = true,
            ExpandedInformation = e.ToString()
        };
        dialog.HyperlinkClicked += (_, _) =>
        {
            Helpers.Open(path);
        };
        return dialog;
    }

    public static Dialog MakeScriptDeletionConfirmation() => new(Button.Yes, Button.No)
    {
        AllowDialogCancellation = true,
        MainIcon = TaskDialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmScriptDeletionContent,
        DefaultButton = Button.No
    };
}