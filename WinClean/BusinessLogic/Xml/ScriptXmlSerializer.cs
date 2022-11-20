using System.Xml;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Xml;

public sealed class ScriptXmlSerializer : IScriptSerializer
{
    public Script Deserialize(ScriptType type, Stream data)
    {
        XmlDocument d = new();
        try
        {
            d.Load(data);
            return new(localizedName: d.GetLocalizedString("Name"),
                       localizedDescription: d.GetLocalizedString("Description"),
                       category: AppInfo.Categories.Value[d.GetSingleChild("Category")],
                       recommendationLevel: AppInfo.RecommendationLevels.Value[d.GetSingleChild("Recommended")],
                       host: AppInfo.Hosts.Value[d.GetSingleChild("Host")],
                       impact: AppInfo.Impacts.Value[d.GetSingleChild("Impact")],
                       code: d.GetSingleChild("Code"),
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