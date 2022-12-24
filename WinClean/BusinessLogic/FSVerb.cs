using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic;

public sealed class FSVerb
{
    private FSVerb(string verb) => Verb = verb;

    /// <summary>Access of a file system element.</summary>
    public static FSVerb Access { get; } = new(FileSystem.Acess);

    /// <summary>Creation of a file system element.</summary>
    public static FSVerb Create { get; } = new(FileSystem.Create);

    /// <summary>Deletion of a file system element.</summary>
    public static FSVerb Delete { get; } = new(FileSystem.Delete);

    /// <summary>Move of a file system element.</summary>
    public static FSVerb Move { get; } = new(FileSystem.Move);

    public string Verb { get; }
}