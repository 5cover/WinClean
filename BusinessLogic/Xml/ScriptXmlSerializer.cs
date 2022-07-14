using Scover.WinClean.BusinessLogic.Scripts;

using System.Xml;

namespace Scover.WinClean.BusinessLogic.Xml;

public class ScriptXmlSerializer : IScriptSerializer
{
    /// <summary>Deserializes a script from XML code.</summary>
    /// <param name="data">A <see cref="string"/> containing the XML code.</param>
    /// <inheritdoc/>
    public Script Deserialize(Stream data)
    {
        XmlDocument doc = new();

        try
        {
            doc.Load(data);

            return new Script(GetLocalizedText("Name"),
                              GetLocalizedText("Description"),
                              GetNode("Code").InnerText,
                              ScriptMetadataFactory.GetCategory(GetNode("Category").InnerText),
                              ScriptMetadataFactory.GetRecommendationLevel(GetNode("Recommended").InnerText),
                              ScriptMetadataFactory.GetImpact(GetNode("Impact").InnerText),
                              ScriptMetadataFactory.GetHost(GetNode("Host").InnerText));
        }
        catch (Exception e) when (e is XmlException or ArgumentException)
        {
            throw new InvalidDataException("The script could not be deserialized because it has invalid or missing data", e);
        }
        LocalizedString GetLocalizedText(string name)
        {
            LocalizedString localizedNodeTexts = new();
            foreach (XmlElement element in doc.GetElementsByTagName(name))
            {
                localizedNodeTexts.SetFromXml(element);
            }
            return localizedNodeTexts;
        }

        XmlNode GetNode(string name)
        {
            XmlNodeList correspondingElements = doc.GetElementsByTagName(name);
            return correspondingElements.Count > 1
                ? throw new ArgumentException($"Multiple \"{name}\" elements in XML document.")
                : correspondingElements[0] ?? throw new ArgumentException($"\"{name}\" element missing from XML document.");
        }
    }

    /// <inheritdoc/>
    public void Serialize(Script script, Stream stream)
    {
        XmlDocument doc = new();

        _ = doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));

        XmlElement? root = doc.CreateElement("Script");

        foreach ((string lang, string text) in script.LocalizedNames)
        {
            Append("Name", text, ("xml:lang", lang));
        }

        foreach ((string lang, string text) in script.LocalizedDescriptions)
        {
            Append("Description", text, ("xml:lang", lang));
        }

        Append("Category", script.Category.InvariantName);
        Append("Recommended", script.Recommended.InvariantName);
        Append("Host", script.Host.InvariantName);
        Append("Impact", script.Impact.InvariantName);
        Append("Code", script.Code);

        _ = doc.AppendChild(root);

        void Append(string name, string? innerText, params (string name, string? value)[] attributes)
        {
            XmlElement e = doc.CreateElement(name);
            foreach ((string name, string? value) attr in attributes)
            {
                e.SetAttribute(attr.name, attr.value);
            }
            e.InnerText = innerText ?? string.Empty;
            _ = root.AppendChild(e);
        }
        doc.Save(stream);
    }
}