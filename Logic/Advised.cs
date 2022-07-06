using Scover.WinClean.Operational;

namespace Scover.WinClean.Logic;

public class Advised
{
    private readonly string _name;

    private Advised(string name, string localizedName)
    {
        _name = name;
        LocalizedName = localizedName;
    }

    /// <summary>The script is generally safe, but it may hinder features that are useful for a minority of users.</summary>
    public static Advised Limited { get; } = new(nameof(Limited), Resources.Advised.Limited);

    /// <summary>
    /// The script is only advised to a minority of users who want advanced optimization. It will almost certainly hinder useful
    /// system features. It should be selected by the user, only if specifically needed.
    /// </summary>
    public static Advised No { get; } = new(nameof(No), Resources.Advised.No);

    public static IEnumerable<Advised> Values => new Advised[] { Yes, Limited, No };

    /// <summary>
    /// The script is advised for any user. It has almost no side effects and won't hinder features that said user might want to
    /// use. It can be selected automatically.
    /// </summary>
    public static Advised Yes { get; } = new(nameof(Yes), Resources.Advised.Yes);

    public string LocalizedName { get; }

    /// <summary>Gets the <see cref="Advised"/> value matching the specified name.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> does not match to any <see cref="Advised"/> name.</exception>
    public static Advised ParseName(string name)
        => Values.SingleOrDefault(validValue => validValue._name == name)
           ?? throw new ArgumentException(Resources.DevException.InvalidTypeProp.FormatWithInvariant(nameof(Advised), nameof(_name)), nameof(name));

    public override string ToString() => _name;
}