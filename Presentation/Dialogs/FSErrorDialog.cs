using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation.Dialogs;

public class FSErrorDialog : Dialog
{    /// <summary>
     /// Initializes a new instance of the <see cref="FSErrorDialog"/> class. </summary> <param name="e">The exception
     /// responsible of the filesystem error.</param> <param name="verb">The filesystem verb that could apply to what was trying
     /// to be done.</param> <param name="info">The file or directory on which the operation was applying.</param> <inheritdoc
     /// cref="Dialog(IEnumerable{Button})" path="/param"/> <remarks>Also sets the following properties: <br><see
     /// cref="TaskDialog.MainIcon"/> to <see cref="TaskDialogIcon.Error"/>;</br> <br><see cref="TaskDialog.WindowTitle"/> to
     /// <see cref="AppInfo.Name"/>;</br> <br><see cref="TaskDialog.CenterParent"/> to <see langword="true"/>.</br> <br><see
     /// cref="TaskDialog.Content"/> to a formatted localized error message.</br></remarks>
    public FSErrorDialog(Exception e, FSVerb verb, FileSystemInfo info, IEnumerable<Button> buttons) : base(buttons)
    {
        MainIcon = TaskDialogIcon.Error;
        Content = Resources.UI.Dialogs.FSErrorContent.FormatWith(verb.LocalizedVerb,
                                                                    info is FileInfo
                                                                        ? FileSystemElements.File
                                                                        : FileSystemElements.Directory,
                                                                    info.FullName,
                                                                    e.Message);
    }

    /// <inheritdoc cref="FSErrorDialog(Exception, FSVerb, FileSystemInfo, IEnumerable{Button})"/>
    public FSErrorDialog(Exception e, FSVerb verb, FileSystemInfo info, params Button[] buttons) : this(e, verb, info, (IEnumerable<Button>)buttons)
    {
    }
}