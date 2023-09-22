using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class FSVerb : Metadata
{
    private FSVerb(string resourceName) : base(new ResourceTextProvider(FSVerbs.ResourceManager, resourceName))
    {
    }

    /// <summary>Access of a file system element.</summary>
    public static FSVerb Access { get; } = new(nameof(FSVerbs.Acess));

    /// <summary>Creation of a file system element.</summary>
    public static FSVerb Create { get; } = new(nameof(FSVerbs.Create));

    /// <summary>Deletion of a file system element.</summary>
    public static FSVerb Delete { get; } = new(nameof(FSVerbs.Delete));

    /// <summary>Move of a file system element.</summary>
    public static FSVerb Move { get; } = new(nameof(FSVerbs.Move));
}