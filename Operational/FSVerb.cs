namespace Scover.WinClean.Operational;

public class FSVerb
{
    private FSVerb(string name, string localizedVerb)
    {
        Name = name;
        LocalizedVerb = localizedVerb;
    }

    /// <summary>Access of a file system element.</summary>
    public static FSVerb Acess { get; } = new(nameof(Acess), Resources.FileSystemVerbs.Acess);

    /// <summary>Creation of a file system element.</summary>
    public static FSVerb Create { get; } = new(nameof(Create), Resources.FileSystemVerbs.Create);

    /// <summary>Deletion of a file system element.</summary>
    public static FSVerb Delete { get; } = new(nameof(Delete), Resources.FileSystemVerbs.Delete);

    /// <summary>Move of a file system element.</summary>
    public static FSVerb Move { get; } = new(nameof(Move), Resources.FileSystemVerbs.Move);

    public static IEnumerable<FSVerb> Values => new[] { Create, Delete, Move, Acess };
    public string LocalizedVerb { get; }
    public string Name { get; }
}