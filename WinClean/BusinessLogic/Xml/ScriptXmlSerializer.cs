using System.Xml;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;

using Semver;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptXmlSerializer : IScriptSerializer
{
    private readonly TypedEnumerablesDictionary _metadatas;

    private readonly SemVersionRange _defaultSupportedVersionRange;

    public ScriptXmlSerializer(TypedEnumerablesDictionary metadatas, SemVersionRange defaultSupportedVersionRange)
        => (_metadatas, _defaultSupportedVersionRange) = (metadatas, defaultSupportedVersionRange);

    #region Element names

    private const string Category = "Category";
    private const string Host = "Host";
    private const string Impact = "Impact";
    private const string Recommended = "Recommended";
    private const string Description = "Description";
    private const string Name = "Name";
    private const string Versions = "Versions";
    private const string Code = "Code";

    #endregion Element names

    public Script Deserialize(ScriptType type, Stream data)
    {
        XmlDocument d = new();

        try
        {
            d.Load(data);

            // Supported Windows versions range in the SemVer 2.0.0 standard version range syntax.
            string? supportedVersionsRange = d.GetSingleChildOrDefault("Versions");

            return new(category: _metadatas.Get<Category>().Single(c => c.InvariantName == d.GetSingleChild(Category)),
                       host: _metadatas.Get<Host>().Single(h => h.InvariantName == d.GetSingleChild(Host)),
                       impact: _metadatas.Get<Impact>().Single(i => i.InvariantName == d.GetSingleChild(Impact)),
                       recommendationLevel: _metadatas.Get<RecommendationLevel>().Single(r => r.InvariantName == d.GetSingleChild(Recommended)),
                       supportedVersionsRange: supportedVersionsRange is null ? _defaultSupportedVersionRange : SemVersionRange.Parse(supportedVersionsRange),
                       localizedDescription: d.GetLocalizedString(Description),
                       localizedName: d.GetLocalizedString(Name),
                       code: d.GetSingleChild(Code),
                       type: type);
        }
        catch (Exception e) when (e is XmlException or KeyNotFoundException)
        {
            throw new InvalidDataException("The script could not be deserialized because it is in a invalid or incomplete format.", e);
        }
    }

    public void Serialize(Script script, Stream stream)
    {
        XmlDocument d = new();

        // Explicit UTF-8 for clarity
        _ = d.AppendChild(d.CreateXmlDeclaration("1.0", "UTF-8", null));

        XmlElement? root = d.CreateElement("Script");

        foreach ((string lang, string text) in script.LocalizedName)
        {
            Append(Name, text, lang);
        }

        foreach ((string lang, string text) in script.LocalizedDescription)
        {
            Append(Description, text, lang);
        }

        Append(Category, script.Category.InvariantName);
        Append(Recommended, script.RecommendationLevel.InvariantName);
        Append(Host, script.Host.InvariantName);
        Append(Impact, script.Impact.InvariantName);
        Append(Versions, script.Versions.ToString());
        Append(Code, script.Code);

        _ = d.AppendChild(root);
        d.Save(stream);

        void Append(string name, string? innerText, string xmlLang = "")
        {
            XmlElement e = d.CreateElement(name);
            if (xmlLang != "")
            {
                e.SetAttribute("xml:lang", xmlLang);
            }
            e.InnerText = innerText ?? "";
            _ = root.AppendChild(e);
        }
    }
}