using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Resources;
using static Scover.WinClean.Resources.UI.Dialogs;

namespace Scover.WinClean.Presentation;

/// <summary>Implements factory methods for commonly used dialog pages.</summary>
public static class DialogPageFactory
{
    /// <summary>Makes a dialog page for deleting a script.</summary>
    /// <returns>A new <see cref="DialogPage"/> object.</returns>
    public static Page MakeDeleteScript()
    {
        return new Page()
        {
            Buttons = new(defaultItem: Button.No)
            {
                Button.Yes,
                Button.No
            },
            Content = ConfirmScriptDeletionContent,
            Icon = DialogIcon.Warning,
            IsCancelable = true,
        };
    }

    /// <summary>Makes a dialog page for a filesystem error.</summary>
    /// <param name="e">The exception responsible for the filesystem error.</param>
    /// <param name="verb">The filesystem verb that could describe what was happening.</param>
    /// <param name="info">The filesystem element that was operated.</param>
    /// <remarks>
    /// Also sets the following properties: <br><see cref="DialogPage.Icon"/> to <see cref="PageIcon.Error"/>;</br><br><see
    /// cref="DialogPage.Content"/> to a formatted and localized error message.</br>
    /// </remarks>
    /// <inheritdoc cref="DialogPage(CustomButton[])" path="/param"/>
    /// <returns>A new <see cref="DialogPage"/> object.</returns>
    public static Page MakeFSError(Exception e, FSVerb verb, FileSystemInfo info, CommitControlCollection buttons) => new()
    {
        Buttons = buttons,
        Content = FSErrorContent.FormatWith(verb.Verb,
            info is FileInfo ? FileSystem.File : FileSystem.Directory,
            info.FullName,
            e.Message),
        Icon = DialogIcon.Error
    };

    /// <summary>Makes a dialog page for a script that could not be loaded because it has invalid or missing data.</summary>
    /// <param name="e">The exception responsible for the error.</param>
    /// <param name="path">The path to the invalid script file.</param>
    /// <inheritdoc cref="DialogPage(CustomButton[])" path="/param"/>
    /// <returns>A new <see cref="DialogPage"/> object.</returns>
    public static Page MakeInvalidScriptData(Exception e, string path, CommitControlCollection buttons)
    {
        Page page = new()
        {
            AllowHyperlinks = true,
            Buttons = buttons,
            Content = InvalidCustomScriptDataContent.FormatWith(Path.GetFileName(path)),
            Expander = new(e.ToString()),
            Icon = DialogIcon.Error,
            MainInstruction = InvalidCustomScriptDataMainInstruction
        };
        page.HyperlinkClicked += (_, _) => path.Open();
        return page;
    }
}