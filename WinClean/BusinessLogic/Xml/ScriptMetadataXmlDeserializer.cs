using System.Windows.Media;
using System.Xml;

using Scover.WinClean.DataAccess;

using WinCopies.Linq;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    private readonly XmlDocument _doc = new();

    public IEnumerable<Category> MakeCategories(Stream stream)
        => from c in GetElements(stream, "Category")
           let _ = GetLocalizables(c)
           select new Category(_.name, _.description);

    public IEnumerable<Host> MakeHosts(Stream stream)
        => from h in GetElements(stream, "Host")
           let _ = GetLocalizables(h)
           select new Host(_.name, _.description, h.GetSingleChild("Executable"), h.GetSingleChild("Arguments"), h.GetSingleChild("Extension"));

    public IEnumerable<Impact> MakeImpacts(Stream stream)
        => from i in GetElements(stream, "Impact")
           let _ = GetLocalizables(i)
           select new Impact(_.name, _.description);

    public IEnumerable<RecommendationLevel> MakeRecommendationLevels(Stream stream)
        => from r in GetElements(stream, "RecommendationLevel")
           let _ = GetLocalizables(r)
           select new RecommendationLevel(_.name, _.description, (Color)ColorConverter.ConvertFromString(r.GetAttribute("Color")));

    private static (LocalizedString name, LocalizedString description) GetLocalizables(XmlNode element)
    {
        LocalizedString name = new(), description = new();
        foreach (XmlElement child in element.ChildNodes)
        {
            switch (child.Name)
            {
            case "Name":
                name.SetFromXml(child);
                break;

            case "Description":
                description.SetFromXml(child);
                break;
            }
        }
        return (name, description);
    }

    private IEnumerable<XmlElement> GetElements(Stream s, string name)
    {
        _doc.Load(s);
        return _doc.GetElementsByTagName(name).Cast<XmlElement>();
    }
}