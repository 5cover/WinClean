using System.Windows.Media;
using System.Xml;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    private readonly XmlDocument _doc = new();

    public IEnumerable<Category> GetCategories(Stream stream)
        => from category in GetLocalizable(stream, "Category")
           select new Category(category.name, category.description);

    public IEnumerable<Host> GetHosts(Stream stream)
        => from host in GetLocalizable(stream, "Host")
           select new Host(host.name, host.description, host.element.GetSingleChild("Executable"), host.element.GetSingleChild("Arguments"), host.element.GetSingleChild("Extension"));

    public IEnumerable<Impact> GetImpacts(Stream stream)
        => from impact in GetLocalizable(stream, "Impact")
           select new Impact(impact.name, impact.description);

    public IEnumerable<RecommendationLevel> GetRecommendationLevels(Stream stream)
        => from recommendationLevel in GetLocalizable(stream, "RecommendationLevel")
           select new RecommendationLevel(recommendationLevel.name, recommendationLevel.description, (Color)ColorConverter.ConvertFromString(recommendationLevel.element.GetAttribute("Color")));

    private IEnumerable<(XmlElement element, LocalizedString name, LocalizedString description)> GetLocalizable(Stream stream, string elementName)
    {
        foreach (var element in GetElements(stream, elementName))
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
            yield return (element, name, description);
        }
    }

    private IEnumerable<XmlElement> GetElements(Stream s, string name)
    {
        _doc.Load(s);
        return _doc.GetElementsByTagName(name).Cast<XmlElement>();
    }
}