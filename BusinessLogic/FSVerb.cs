using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic;

public class FSVerb
{
    private FSVerb(string localizedVerb)
    {
        LocalizedVerb = localizedVerb;
    }

    /// <summary>Access of a file system element.</summary>
    public static FSVerb Access { get; } = new(FileSystemVerbs.Acess);

    /// <summary>Creation of a file system element.</summary>
    public static FSVerb Create { get; } = new(FileSystemVerbs.Create);

    /// <summary>Deletion of a file system element.</summary>
    public static FSVerb Delete { get; } = new(FileSystemVerbs.Delete);

    /// <summary>Move of a file system element.</summary>
    public static FSVerb Move { get; } = new(FileSystemVerbs.Move);

    public string LocalizedVerb { get; }
}