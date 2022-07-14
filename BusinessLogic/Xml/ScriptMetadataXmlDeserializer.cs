using Scover.WinClean.BusinessLogic.Scripts;

using System.Windows.Media;
using System.Xml;

namespace Scover.WinClean.BusinessLogic.Xml;

public class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    private readonly XmlDocument _doc = new();

    public ScriptMetadataXmlDeserializer(string xml) => _doc.LoadXml(xml);

    public IEnumerable<Category> MakeCategories()
    {
        foreach (XmlElement category in _doc.GetElementsByTagName("Category"))
        {
            (LocalizedString name, LocalizedString description) = GetNameAndDescription(category);
            yield return new Category(name, description);
        }
    }

    public IEnumerable<Impact> MakeImpacts()
    {
        foreach (XmlElement impact in _doc.GetElementsByTagName("Impact"))
        {
            (LocalizedString name, LocalizedString description) = GetNameAndDescription(impact);
            yield return new Impact(name, description);
        }
    }

    public IEnumerable<RecommendationLevel> MakeRecommendationLevels()
    {
        foreach (XmlElement recommendationLevel in _doc.GetElementsByTagName("RecommendationLevel"))
        {
            (LocalizedString name, LocalizedString description) = GetNameAndDescription(recommendationLevel);
            yield return new RecommendationLevel(name, description, (Color)ColorConverter.ConvertFromString(recommendationLevel.GetAttribute("Color")));
        }
    }

    private static (LocalizedString name, LocalizedString description) GetNameAndDescription(XmlNode element)
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
}