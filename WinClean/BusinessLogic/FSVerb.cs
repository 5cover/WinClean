using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic;

public sealed class FSVerb
{
    private FSVerb(string verb) => Name = verb;

    /// <summary>Access of a file system element.</summary>
    public static FSVerb Access { get; } = new(FSVerbs.Acess);

    /// <summary>Creation of a file system element.</summary>
    public static FSVerb Create { get; } = new(FSVerbs.Create);

    /// <summary>Deletion of a file system element.</summary>
    public static FSVerb Delete { get; } = new(FSVerbs.Delete);

    /// <summary>Move of a file system element.</summary>
    public static FSVerb Move { get; } = new(FSVerbs.Move);

    public string Name { get; }
}