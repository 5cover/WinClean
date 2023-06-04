using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Services;

using static Scover.WinClean.Resources.UI.Dialogs;

namespace Scover.WinClean.ViewModel;

/// <summary>Implements factory methods for commonly used dialog pages.</summary>
public static class DialogPages
{
    /// <summary>Gets the dialog page for confirming a abort operation.</summary>
    public static Page ConfirmAbortOperation => new()
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Warning,
        Content = ConfirmAbortOperationContent,
        Buttons = { Button.Yes, Button.No },
    };

    /// <summary>Gets the dialog page for deleting a script.</summary>
    public static Page DeleteScript => new()
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Warning,
        Content = ConfirmScriptDeletionContent,
        Buttons = new(defaultItem: Button.No)
        {
            Button.Yes,
            Button.No
        },
    };

    /// <summary>Makes a dialog page for a filesystem error.</summary>
    /// <param name="e">The exception responsible for the filesystem error.</param>
    /// <param name="verb">The filesystem verb that could describe what was happening.</param>
    /// <param name="info">The filesystem element that was operated.</param>
    /// <remarks>
    /// Also sets the following properties: <br><see cref="Page.Icon"/> to <see
    /// cref="DialogIcon.Error"/>;</br><br><see cref="Page.Content"/> to a formatted and localized error
    /// message.</br>
    /// </remarks>
    /// <returns>A new <see cref="Page"/> object.</returns>
    public static Page FSError(FileSystemException e, ButtonCollection buttons) => new()
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Error,
        Content = FSErrorContent.FormatWith(e.Verb.Name,
            e.Element,
            e.Message),
        Buttons = buttons,
        Expander = new(e.ToString())
    };

    /// <summary>
    /// Makes a dialog page for a script that could not be loaded because it has invalid or missing data.
    /// </summary>
    /// <param name="e">The exception responsible for the error.</param>
    /// <param name="path">The path to the invalid script file.</param>
    /// <returns>A new <see cref="Page"/> object.</returns>
    public static Page InvalidScriptData(Exception e, string path, ButtonCollection buttons)
    {
        Page page = new()
        {
            AllowHyperlinks = true,
            WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
            Icon = DialogIcon.Error,
            MainInstruction = InvalidCustomScriptDataMainInstruction,
            Content = InvalidCustomScriptDataContent.FormatWith(Path.GetFileName(path)),
            Expander = new(e.ToString()),
            Buttons = buttons,
        };
        page.HyperlinkClicked += (_, _) => path.Open();
        return page;
    }
}