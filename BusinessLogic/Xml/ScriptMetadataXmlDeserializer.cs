﻿using System.Windows.Media;
using System.Xml;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    private readonly XmlDocument _doc = new();

    public IEnumerable<Category> MakeCategories(Stream stream)
    {
        _doc.Load(stream);
        foreach (XmlElement category in _doc.GetElementsByTagName("Category"))
        {
            var (name, description) = GetNameAndDescription(category);
            yield return new Category(name, description);
        }
    }

    public IEnumerable<Host> MakeHosts(Stream stream)
    {
        _doc.Load(stream);
        foreach (XmlElement host in _doc.GetElementsByTagName("Host"))
        {
            var (name, description) = GetNameAndDescription(host);
            yield return new Host(name, description, host.GetSingleNode("Executable"), host.GetSingleNode("Arguments"), host.GetSingleNode("Extension"));
        }
    }

    public IEnumerable<Impact> MakeImpacts(Stream stream)
    {
        _doc.Load(stream);
        foreach (XmlElement impact in _doc.GetElementsByTagName("Impact"))
        {
            var (name, description) = GetNameAndDescription(impact);
            yield return new Impact(name, description);
        }
    }

    public IEnumerable<RecommendationLevel> MakeRecommendationLevels(Stream stream)
    {
        _doc.Load(stream);
        foreach (XmlElement recommendationLevel in _doc.GetElementsByTagName("RecommendationLevel"))
        {
            var (name, description) = GetNameAndDescription(recommendationLevel);
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