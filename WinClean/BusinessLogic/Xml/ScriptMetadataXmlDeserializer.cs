using System.Windows.Media;
using System.Xml;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    private readonly XmlDocument _doc = new();

    public IEnumerable<Category> GetCategories(Stream stream)
        => from category in GetElements(stream, "Category")
           let localizable = GetLocalizable(category)
           select new Category(localizable.name, localizable.description);

    public IEnumerable<Host> GetHosts(Stream stream)
        => from host in GetElements(stream, "Host")
           let localizable = GetLocalizable(host)
           select new Host(localizable.name, localizable.description, host.GetSingleChild("Executable"), host.GetSingleChild("Arguments"), host.GetSingleChild("Extension"));

    public IEnumerable<Impact> GetImpacts(Stream stream)
        => from impact in GetElements(stream, "Impact")
           let localizable = GetLocalizable(impact)
           select new Impact(localizable.name, localizable.description);

    public IEnumerable<RecommendationLevel> GetRecommendationLevels(Stream stream)
        => from recommendationLevel in GetElements(stream, "RecommendationLevel")
           let _ = GetLocalizable(recommendationLevel)
           select new RecommendationLevel(_.name, _.description, (Color)ColorConverter.ConvertFromString(recommendationLevel.GetAttribute("Color")));

    private static (LocalizedString name, LocalizedString description) GetLocalizable(XmlNode element)
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