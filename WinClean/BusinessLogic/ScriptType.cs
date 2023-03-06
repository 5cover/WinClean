using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic;

public sealed class ScriptType
{
    private ScriptType(string name, bool isMutable)
        => (Name, IsMutable) = (name, isMutable);

    public static ScriptType Custom { get; } = new(ScriptTypes.Custom, true);
    public static ScriptType Default { get; } = new(ScriptTypes.Default, false);
    public bool IsMutable { get; }
    public string Name { get; }
}