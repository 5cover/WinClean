using Scover.WinClean.Operational;

using System.Globalization;
using System.Xml;

namespace Scover.WinClean.Logic;

public class ScriptXmlSerializer : IScriptSerializer
{
    private const string LCIDTagNamePrefix = "lcid";
    private static readonly Dictionary<string, (Dictionary<int, string> Names, Dictionary<int, string> Descriptions)> _localizedScriptsData = new();
    private readonly DirectoryInfo _scriptsDir;

    public ScriptXmlSerializer(DirectoryInfo scriptsDir) => _scriptsDir = scriptsDir;

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

        var value = (GetLocalizedNodeTexts("Name"), GetLocalizedNodeTexts("Description"));
        if (!_localizedScriptsData.TryAdd(filename, value))
        {
            // If the value already exists, update it.
            _localizedScriptsData[filename] = value;
        }

        return new Script
        (
            advised: Advised.ParseName(GetNodeText("Advised")),
            category: Category.ParseName(GetNodeText("Category")),
            code: GetNodeText("Code"),
            description: GetLocalizedText(_localizedScriptsData[filename].Descriptions),
            host: ScriptHostFactory.FromName(GetNodeText("Host")),
            filename: filename,
            impact: Impact.ParseName(GetNodeText("Impact")),
            name: GetLocalizedText(_localizedScriptsData[filename].Names)
        );

        Dictionary<int, string> GetLocalizedNodeTexts(string name)
        {
            Dictionary<int, string> localizedNodeTexts = new();
            foreach (XmlNode child in GetNode(name).ChildNodes)
            {
                localizedNodeTexts.Add(int.Parse(child.Name.Remove(0, LCIDTagNamePrefix.Length), CultureInfo.CurrentCulture), GetText(child));
            }
            return localizedNodeTexts;
        }

        string GetLocalizedText(Dictionary<int, string> texts)
        {
            string? localized;
            for (CultureInfo culture = CultureInfo.CurrentUICulture; !texts.TryGetValue(culture.LCID, out localized); culture = culture.Parent)
            {
                // This is not in the condition section of the for loop because it would prevent iterating on InvariantCulture.
                if (culture == CultureInfo.InvariantCulture)
                {
                    break;
                }
            }
            return localized ?? string.Empty;
        }

        XmlNode GetNode(string name)
        {
            XmlNodeList correspondingElements = doc.GetElementsByTagName(name);
            return correspondingElements.Count > 1
                ? throw new ArgumentException($"Multiple \"{name}\" elements in XML document.")
                : correspondingElements[0] ?? throw new ArgumentException($"\"{name}\" element missing from XML document.");
        }

        string GetNodeText(string name) => GetText(GetNode(name));
        string GetText(XmlNode node) => node.InnerText.Trim();
    }

    public void Serialize(Script s)
    {
        XmlDocument doc = new();

        _ = doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));

        XmlElement root = doc.CreateElement(nameof(Script));

        {
            XmlElement name = doc.CreateElement("Name");
            foreach ((int lcid, string text) in _localizedScriptsData[s.Filename].Names)
            {
                CreateAppend(name, GetLCIDTagName(lcid), text);
            }
            _ = root.AppendChild(name);
        }

        {
            XmlElement description = doc.CreateElement("Description");
            foreach ((int lcid, string text) in _localizedScriptsData[s.Filename].Descriptions)
            {
                CreateAppend(description, GetLCIDTagName(lcid), text);
            }
            _ = root.AppendChild(description);
        }

        CreateAppend(root, "Category", s.Category.ToString());
        CreateAppend(root, "Advised", s.Advised.ToString());
        CreateAppend(root, "Host", s.Host.Name);
        CreateAppend(root, "Impact", s.Impact.ToString());
        CreateAppend(root, "Code", s.Code);

        _ = doc.AppendChild(root);

        void CreateAppend(XmlElement parent, string name, string? innerText)
        {
            XmlElement e = doc.CreateElement(name);
            e.InnerText = innerText ?? string.Empty;
            _ = parent.AppendChild(e);
        }

        string GetLCIDTagName(int lcid)
            => LCIDTagNamePrefix + lcid.ToString(CultureInfo.InvariantCulture);

        doc.Save(Path.Join(_scriptsDir.FullName, $"{Path.GetFileNameWithoutExtension(s.Filename)}.xml"));
    }
}