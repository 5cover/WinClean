using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation.Dialogs;

public static class FSErrorFactory
{
    /// <summary>Creates a new instance of <typeparamref name="T"/> and makes it a filesystem error.</summary>
    /// <typeparam name="T">A <see cref="TaskDialog"/> or a subtype that can be publicly instanciated.</typeparam>
    /// <param name="e">The exception responsible of the filesystem error.</param>
    /// <param name="verb">The filesystem verb that could apply to what was trying to be done.</param>
    /// <param name="info">The file or directory on which the operation was applying.</param>
    /// <returns>A new instance of <typeparamref name="T"/> with the data of a filesystem error.</returns>
    public static T MakeFSError<T>(Exception e, FSVerb verb, FileSystemInfo info) where T : TaskDialog, new()
        => new()
        {
            MainIcon = TaskDialogIcon.Error,
            Content = Resources.UI.Dialogs.FSErrorContent.FormatWith(verb.LocalizedVerb,
                                                                     info is FileInfo
                                                                         ? Resources.FileSystemElements.File
                                                                         : Resources.FileSystemElements.Directory,
                                                                     info.FullName,
                                                                     e.Message)
        };
}