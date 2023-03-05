using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>Represents the type of a script.</summary>
public sealed class ScriptType
{
    private ScriptType(string name, bool isEditable) => (Name, IsEditable) = (name, isEditable);

    public static ScriptType Custom { get; } = new(ScriptTypes.Custom, true);

    public static ScriptType Default { get; } = new(ScriptTypes.Default, false);

    public string Name { get; }
    public bool IsEditable { get; }
}