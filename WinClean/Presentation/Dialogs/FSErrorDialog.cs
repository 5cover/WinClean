using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation.Dialogs;

public sealed class FSErrorDialog : Dialog
{
    /// <param name="e">The exception responsible of the filesystem error.</param>

    /// <param name="verb">The filesystem verb that could apply to what was trying to be done.</param>
    /// <param name="info">The file or directory on which the operation was applying.</param>
    /// <remarks>
    /// Also sets the following properties: <br><see cref="Dialog.MainIcon"/> to <see
    /// cref="TaskDialogIcon.Error"/>;</br><br><see cref="Dialog.Content"/> to a formatted and localized error message.</br>
    /// </remarks>
    /// <inheritdoc cref="Dialog(IEnumerable{Button})" path="/param"/>
    public FSErrorDialog(Exception e, FSVerb verb, FileSystemInfo info, params Button[] buttons) : base(buttons)
    {
        MainIcon = TaskDialogIcon.Error;
        Content = Resources.UI.Dialogs.FSErrorContent.FormatWith(verb.LocalizedVerb,
                                                                    info is FileInfo
                                                                        ? FileSystemElements.File
                                                                        : FileSystemElements.Directory,
                                                                    info.FullName,
                                                                    e.Message);
    }
}