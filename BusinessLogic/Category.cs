using Scover.WinClean.DataAccess;

using System.Globalization;
using System.Xml;

namespace Scover.WinClean.BusinessLogic;

public class Category
{
    private readonly Localized<string> _description;

    private readonly Localized<string> _name;

    private Category(Localized<string> name, Localized<string> description)
    {
        _name = name;
        _description = description;
    }

    public static IEnumerable<Category> Values { get; } = LoadCategories();
    public string Description => _description.Get(CultureInfo.CurrentUICulture);
    public string Name => _name.Get(CultureInfo.CurrentUICulture);

    /// <summary>Gets the <see cref="Category"/> matching the specified name in an invariant culture.</summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> does not match to any <see cref="Category"/> name. --OR-- <paramref name="culture"/> was not found.
    /// </exception>
    public static Category FromName(string name) => Values.SingleOrDefault(value => value._name.Get(CultureInfo.InvariantCulture) == name)
           ?? throw new ArgumentException(Resources.DevException.InvalidTypeProp.FormatWithInvariant(nameof(Category), nameof(_name)), nameof(name));

    public override string ToString() => _name.Get(CultureInfo.InvariantCulture);

    private static IEnumerable<Category> LoadCategories()
    {
        List<Category> categories = new();

        XmlDocument doc = new();
        doc.LoadXml(File.ReadAllText(AppDirectory.InstallDir.Join("Categories.xml")));

        foreach (XmlElement element in doc.GetElementsByTagName("Category"))
        {
            Localized<string> name = new();
            Localized<string> description = new();

            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name == "Name")
                {
                    name.SetFromXml(child);
                }
                else if (child.Name == "Description")
                {
                    description.SetFromXml(child);
                }
            }
            categories.Add(new(name, description));
        }

        return categories;
    }
}