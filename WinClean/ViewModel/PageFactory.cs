using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel;

/// <summary>Implements factory methods for commonly used dialog pages.</summary>
public static class PageFactory
{
    public static bool Confirm(Func<Page> pageFactory)
    {
        using Page page = pageFactory();
        return Button.Yes.Equals(new Dialog(page).Show());
    }

    /// <summary>Gets the dialog page for confirming a abort operation.</summary>
    /// <remarks>Buttons: <see cref="Button.Yes"/>, <see cref="Button.No"/>.</remarks>
    public static Page MakeConfirmAbortOperation() => new()
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmAbortOperationContent,
        Buttons = { Button.Yes, Button.No },
    };

    /// <summary>Gets the dialog page for confirming the deletion of a script.</summary>
    /// <remarks>Buttons: <see cref="Button.Yes"/>, <see cref="Button.No"/>.</remarks>
    public static Page MakeConfirmScriptDeletion() => new()
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmScriptDeletionContent,
        Buttons = new()
        {
            Button.Yes,
            Button.No
        },
    };

    /// <summary>Makes a dialog page for a filesystem error.</summary>
    /// <remarks>
    /// Also sets the following properties: <br><see cref="Page.Icon"/> to <see
    /// cref="DialogIcon.Error"/>;</br><br><see cref="Page.Content"/> to a formatted and localized error
    /// message.</br>
    /// </remarks>
    /// <returns>A new <see cref="Page"/> object.</returns>
    public static Page MakeFSError(FileSystemException e, ButtonCollection buttons) => new()
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Error,
        Content = Resources.UI.Dialogs.FSErrorContent.FormatWith(e.Verb.Name,
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
    public static Page MakeScriptLoadError(Exception e, string path, ButtonCollection buttons)
    {
        Page page = new()
        {
            AllowHyperlinks = true,
            Buttons = buttons,
            Content = Resources.UI.Dialogs.ScriptLoadErrorContent.FormatWith(Path.GetFileName(path)),
            Expander = new(e.ToString()),
            Icon = DialogIcon.Error,
            MainInstruction = Resources.UI.Dialogs.ScriptLoadErrorMainInstruction,
            Sizing = Sizing.Content,
            WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        };
        page.HyperlinkClicked += (_, _) => path.Open();
        return page;
    }
}