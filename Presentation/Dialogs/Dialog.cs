using Ookii.Dialogs.Wpf;

namespace Scover.WinClean.Presentation.Dialogs;

/// <summary>The root of the Dialog class tree. Inherits from Ookii's <see cref="TaskDialog"/>.</summary>
public abstract class Dialog : TaskDialog
{
    protected Dialog()
    {
        WindowTitle = App.Name;
        CenterParent = true;
    }
}