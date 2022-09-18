using System.Xml;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptXmlSerializer : IScriptSerializer
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

            string? executionTime = GetOptionalNode("ExecutionTime");

            return new Script(AppInfo.Categories[GetNode("Category")],
                              GetNode("Code"),
                              executionTime is null ? AppInfo.Settings.ScriptTimeout : TimeSpan.Parse(executionTime, System.Globalization.CultureInfo.InvariantCulture),
                              AppInfo.Hosts[GetNode("Host")],
                              AppInfo.Impacts[GetNode("Impact")],
                              AppInfo.RecommendationLevels[GetNode("Recommended")],
                              GetLocalizedText("Description"),
                              GetLocalizedText("Name"));
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

        string GetNode(string name) => GetNodeInternal(name, node => node?.InnerText ?? throw new ArgumentException($"\"{name}\" element missing from XML document."));

        T GetNodeInternal<T>(string name, Func<XmlNode?, T> getNodeText)
        {
            XmlNodeList correspondingElements = doc.GetElementsByTagName(name);
            return correspondingElements.Count > 1
                ? throw new ArgumentException($"Multiple \"{name}\" elements in XML document.")
                : getNodeText(correspondingElements[0]);
        }

        string? GetOptionalNode(string name) => GetNodeInternal(name, node => node?.InnerText);
    }

    /// <inheritdoc/>
    public void Serialize(Script script, Stream stream)
    {
        XmlDocument doc = new();

        // Explicit UTF-8 for clarity
        _ = doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

        XmlElement? root = doc.CreateElement("Script");

        foreach ((string lang, string text) in script.LocalizedNames)
        {
            Append("Name", text, lang);
        }

        foreach ((string lang, string text) in script.LocalizedDescriptions)
        {
            Append("Description", text, lang);
        }

        Append("Category", script.Category.InvariantName);
        Append("Recommended", script.Recommended.InvariantName);
        Append("Host", script.Host.InvariantName);
        Append("Impact", script.Impact.InvariantName);
        Append("Code", script.Code);
        if (script.ExecutionTime != TimeSpan.Zero)
        {
            Append("ExecutionTime", script.ExecutionTime.ToString("c"));
        }

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
}