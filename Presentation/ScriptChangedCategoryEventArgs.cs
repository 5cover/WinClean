using Scover.WinClean.Logic;

namespace Scover.WinClean.Presentation;

public class ScriptChangedCategoryEventArgs : EventArgs
{
    public ScriptChangedCategoryEventArgs(Script script, Category oldCategory, Category newCategory)
    {
        Script = script;
        OldCategory = oldCategory;
        NewCategory = newCategory;
    }

    public Category NewCategory { get; }
    public Category OldCategory { get; }
    public Script Script { get; }
}