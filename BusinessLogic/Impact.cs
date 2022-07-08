using Scover.WinClean.DataAccess;

using System.Globalization;
using System.Xml;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Effect of running a script.</summary>
public class Impact
{
    private readonly Localized<string> _description;

    private readonly Localized<string> _name;

    private Impact(Localized<string> name, Localized<string> description)
    {
        _name = name;
        _description = description;
    }

    public static IEnumerable<Impact> Values { get; } = LoadImpacts();
    public string Description => _description.Get(CultureInfo.CurrentUICulture);
    public string Name => _name.Get(CultureInfo.CurrentUICulture);

    /// <summary>Gets the <see cref="Impact"/> matching the specified name in an invariant culture.</summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> does not match to any <see cref="Impact"/> name. --OR-- <paramref name="culture"/> was not found.
    /// </exception>
    public static Impact FromName(string name) => Values.SingleOrDefault(value => value._name.Get(CultureInfo.InvariantCulture) == name)
           ?? throw new ArgumentException(Resources.DevException.InvalidTypeProp.FormatWithInvariant(nameof(Impact), nameof(_name)), nameof(name));

    public override string ToString() => _name.Get(CultureInfo.InvariantCulture);

    private static IEnumerable<Impact> LoadImpacts()
    {
        List<Impact> impacts = new();

        XmlDocument doc = new();
        doc.LoadXml(File.ReadAllText(AppDirectory.InstallDir.Join("Impacts.xml")));

        foreach (XmlElement element in doc.GetElementsByTagName("Impact"))
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
            impacts.Add(new(name, description));
        }

        return impacts;
    }
}