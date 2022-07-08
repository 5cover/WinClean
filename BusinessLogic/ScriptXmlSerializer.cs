using Scover.WinClean.BusinessLogic.ScriptExecution;
using Scover.WinClean.DataAccess;

using System.Xml;

namespace Scover.WinClean.BusinessLogic;

public class ScriptXmlSerializer : IScriptSerializer
{
    /// <summary>Deserializes a script from an XML file.</summary>
    /// <param name="source">The XML file.</param>
    /// <returns>A new <see cref="Script"/> object.</returns>
    /// <exception cref="ArgumentException"><paramref name="source"/>'s data is missing or invalid.</exception>
    /// <exception cref="XmlException"><paramref name="source"/> is not a valid XML document.</exception>
    /// <exception cref="DirectoryNotFoundException">
    /// <paramref name="source"/> is invalid (for example, it is on an unmapped drive).
    /// </exception>
    /// <exception cref="IOException">An I/O error occurred while opening <paramref name="source"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">
    /// <paramref name="source"/> is readonly. -or- The caller does not have the required permission.
    /// </exception>
    /// <exception cref="FileNotFoundException"><paramref name="source"/> was not found.</exception>
    /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
    public Script Deserialize(FileInfo source)
    {
        XmlDocument doc = new();
        doc.Load(source.FullName);

        string filename = Path.GetFileName(source.Name);

        return new Script
        (
            recommended: RecommendationLevel.FromName(GetNode("Recommended").InnerText),
            category: Category.FromName(GetNode("Category").InnerText),
            code: GetNode("Code").InnerText,
            localizedDescriptions: GetLocalizedText("Description"),
            host: ScriptHostFactory.FromName(GetNode("Host").InnerText),
            filename: filename,
            impact: Impact.FromName(GetNode("Impact").InnerText),
            localizedNames: GetLocalizedText("Name")
        );

        Localized<string> GetLocalizedText(string name)
        {
            Localized<string> localizedNodeTexts = new();
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

    public void Serialize(Script s)
    {
        XmlDocument doc = new();

        _ = doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));

        XmlElement root = doc.CreateElement(nameof(Script));

        foreach ((string lang, string text) in s.LocalizedNames)
        {
            Append("Name", text, ("xml:lang", lang));
        }

        foreach ((string lang, string text) in s.LocalizedDescriptions)
        {
            Append("Description", text, ("xml:lang", lang));
        }

        Append("Category", s.Category.ToString());
        Append("Recommended", s.Recommended.ToString());
        Append("Host", s.Host.Name);
        Append("Impact", s.Impact.ToString());
        Append("Code", s.Code);

        _ = doc.AppendChild(root);

        void Append(string name, string? innerText, params (string name, string? value)[] attributes)
        {
            XmlElement e = doc.CreateElement(name);
            foreach (var attr in attributes)
            {
                e.SetAttribute(attr.name, attr.value);
            }
            e.InnerText = innerText ?? string.Empty;
            _ = root.AppendChild(e);
        }

        doc.Save(Path.Join(AppDirectory.ScriptsDir.Info.FullName, s.Filename));
    }
}