using System.Xml;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptXmlSerializer : IScriptSerializer
{
    /// <summary>Deserializes a script from XML code.</summary>
    /// <param name="data">A <see cref="string"/> containing the XML code.</param>
    /// <inheritdoc/>
    public Script Deserialize(Stream data) => Deserialize(data, false);

    /// <inheritdoc cref="Deserialize(Stream)"/>
    public Script DeserializeDefault(Stream data) => Deserialize(data, true);

    /// <inheritdoc/>
    public void Serialize(Script script, Stream stream)
    {
        XmlDocument doc = new();

        // Explicit UTF-8 for clarity
        _ = doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

        XmlElement? root = doc.CreateElement("Script");

        foreach ((string lang, string text) in script.LocalizedName)
        {
            Append("Name", text, lang);
        }

        foreach ((string lang, string text) in script.LocalizedDescription)
        {
            Append("Description", text, lang);
        }

        Append("Category", script.Category.InvariantName);
        Append("Recommended", script.RecommendationLevel.InvariantName);
        Append("Host", script.Host.InvariantName);
        Append("Impact", script.Impact.InvariantName);
        Append("Code", script.Code);

        _ = doc.AppendChild(root);

        void Append(string name, string? innerText, string xmlLang = "")
        {
            XmlElement e = doc.CreateElement(name);
            if (xmlLang != "")
            {
                e.SetAttribute("xml:lang", xmlLang);
            }
            e.InnerText = innerText ?? string.Empty;
            _ = root.AppendChild(e);
        }
        doc.Save(stream);
    }

    private static Script Deserialize(Stream data, bool isDefault)
    {
        XmlDocument doc = new();
        try
        {
            doc.Load(data);

            return new Script(AppInfo.Categories.Value[doc.GetSingleNode("Category")],
                              doc.GetSingleNode("Code"),
                              AppInfo.Hosts.Value[doc.GetSingleNode("Host")],
                              AppInfo.Impacts.Value[doc.GetSingleNode("Impact")],
                              AppInfo.RecommendationLevels.Value[doc.GetSingleNode("Recommended")],
                              isDefault,
                              GetLocalizedString("Name"),
                              GetLocalizedString("Description"));
        }
        catch (Exception e) when (e is XmlException or ArgumentException or KeyNotFoundException)
        {
            throw new InvalidDataException("The script could not be deserialized because it has invalid or missing data", e);
        }

        LocalizedString GetLocalizedString(string name)
        {
            LocalizedString localizedNodeTexts = new();
            foreach (XmlElement element in doc.GetElementsByTagName(name))
            {
                localizedNodeTexts.SetFromXml(element);
            }
            return localizedNodeTexts;
        }
    }
}