using Ookii.Dialogs.Wpf;

using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources.UI;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>Implements factory methods for commonly used dialogs.</summary>
public static class DialogFactory
{
    public static Dialog MakeDeleteScriptDialog() => new(Button.Yes, Button.No)
    {
        AllowDialogCancellation = true,
        MainIcon = TaskDialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmScriptDeletionContent,
        DefaultButton = Button.No
    };

    public static Dialog MakeInvalidScriptDataDialog(Exception e, string path, params Button[] buttons)
    {
        Dialog dialog = new(buttons)
        {
            MainIcon = TaskDialogIcon.Error,
            MainInstruction = InvalidCustomScriptData.MainInstruction,
            Content = InvalidCustomScriptData.Content.FormatWith(Path.GetFileName(path)),
            AreHyperlinksEnabled = true,
            ExpandedInformation = e.ToString()
        };
        dialog.HyperlinkClicked += (_, _) => Helpers.Open(path);
        return dialog;
    }
}