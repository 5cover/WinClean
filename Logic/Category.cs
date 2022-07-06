using Scover.WinClean.Operational;

namespace Scover.WinClean.Logic;

public class Category
{
    private readonly string _name;

    private Category(string name, string? localizedName)
    {
        LocalizedName = localizedName;
        _name = name;
    }

    public static Category Customization { get; } = new(nameof(Customization), Resources.Category.Customization);
    public static Category Debloat { get; } = new(nameof(Debloat), Resources.Category.Debloat);
    public static Category Maintenance { get; } = new(nameof(Maintenance), Resources.Category.Maintenance);

    public static IEnumerable<Category> Values => new[]
    {
        Maintenance,
        Debloat,
        Customization
    };

    public string? LocalizedName { get; }

    /// <summary>Gets the <see cref="Category"/> matching the specified name.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> does not match to any <see cref="Category"/> name.</exception>
    public static Category ParseName(string name)
        => Values.SingleOrDefault(validValue => validValue._name == name)
           ?? throw new ArgumentException(Resources.DevException.InvalidTypeProp.FormatWithInvariant(nameof(Category), nameof(_name)), nameof(name));

    public override string ToString() => _name;
}