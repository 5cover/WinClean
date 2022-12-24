using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>Represents the type of a script.</summary>
public sealed class ScriptType
{
    private ScriptType(string name) => Name = name;

    public static ScriptType Custom { get; } = new(ScriptTypes.Custom);

    public static ScriptType Default { get; } = new(ScriptTypes.Default);

    public string Name { get; }
}