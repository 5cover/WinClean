using System.Globalization;
using System.Xml;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Services;

using Semver;

namespace Scover.WinClean.Model.Serialization.Xml;

public sealed class ScriptXmlSerializer : IScriptSerializer
{
    private readonly SemVersionRange _defaultSupportedVersionRange;

    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    public ScriptXmlSerializer(SemVersionRange defaultSupportedVersionRange)
        => _defaultSupportedVersionRange = defaultSupportedVersionRange;

    public static class Elements
    {
        public const string
            Category = "Category",
            Code = "Code",
            Description = "Description",
            Host = "Host",
            Impact = "Impact",
            Name = "Name",
            Safety = "Safety",
            Versions = "Versions";
    }

    public Script Deserialize(ScriptType type, Stream data)
    {
        XmlDocument d = new();
        try
        {
            d.Load(data);
            return DeserializeOrDefault(type, d) ?? DeserializePre130(type, d);
        }
        catch (Exception e) when (e is InvalidOperationException or XmlException or KeyNotFoundException or FormatException)
        {
            throw new InvalidDataException("The script could not be deserialized because it is in a invalid or incomplete format.", e);
        }
    }

    /// <summary>
    /// Deserializes a script in the current format.
    /// </summary>
    private Script? DeserializeOrDefault(ScriptType type, XmlDocument d) => DeserializeVersions(d) is { } versions &&
        DeserializeCodeOrDefault(d) is { } code
            ? new Script(Metadatas.GetMetadata<Category>(d.GetSingleChildText(Elements.Category)),
                        Metadatas.GetMetadata<Impact>(d.GetSingleChildText(Elements.Impact)),
                        versions,
                        Metadatas.GetMetadata<SafetyLevel>(d.GetSingleChildText(Elements.Safety)),
                        d.GetLocalizedString(Elements.Description),
                        d.GetLocalizedString(Elements.Name),
                        type,
                        code)
            : null;

    private SemVersionRange DeserializeVersions(XmlDocument d)
        => d.GetSingleChildTextOrDefault(Elements.Versions) is { } versionsStr
            ? SemVersionRange.Parse(versionsStr)
            : _defaultSupportedVersionRange;

    /// <summary>
    /// Deserializes a script in the pre 1.3.0 format.
    /// </summary>
    private Script DeserializePre130(ScriptType type, XmlDocument d) => new(Metadatas.GetMetadata<Category>(d.GetSingleChildText(Elements.Category)),
        Metadatas.GetMetadata<Impact>(d.GetSingleChildText(Elements.Impact)),
        _defaultSupportedVersionRange,
        Metadatas.GetMetadata<SafetyLevel>(d.GetSingleChildText("Recommended") switch
        {
            "Yes" => "Safe",
            "No" => "Dangerous",
            _ => d.GetSingleChildText("Recommended")
        }),
        d.GetLocalizedString(Elements.Description),
        d.GetLocalizedString(Elements.Name),
        type,
        new ScriptCode(new()
        {
            [Capability.Execute] = new ScriptAction(
                host: Metadatas.GetMetadata<Host>(d.GetSingleChildText(Elements.Host)),
                code: d.GetSingleChildText(Elements.Code))
        }));

    public void Serialize(Script script, Stream stream)
    {
        XmlDocument d = new();

        // Explicit UTF-8 for clarity
        _ = d.AppendChild(d.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = d.CreateElement("Script");
        _ = d.AppendChild(root);

        // The append order is significant.

        foreach ((CultureInfo lang, string text) in script.LocalizedName)
        {
            AppendLocalizable(root, Elements.Name, text, lang.Name);
        }

        foreach ((CultureInfo lang, string text) in script.LocalizedDescription)
        {
            AppendLocalizable(root, Elements.Description, text, lang.Name);
        }

        Append(root, Elements.Category, script.Category.InvariantName);
        Append(root, Elements.Safety, script.SafetyLevel.InvariantName);
        Append(root, Elements.Impact, script.Impact.InvariantName);
        Append(root, Elements.Versions, script.Versions.ToString());

        SerializeCode(root, script.Code);

        d.Save(stream);
    }

    private static void Append(XmlElement parent, string name, string? innerText, Dictionary<string, string?>? attributes = null)
    {
        var e = parent.OwnerDocument.CreateElement(name);
        _ = parent.AppendChild(e);

        if (attributes is not null)
        {
            foreach ((string attrName, string? attrValue) in attributes)
            {
                e.SetAttribute(attrName, attrValue);
            }
        }
        e.InnerText = innerText ?? "";
    }

    private static void AppendLocalizable(XmlElement element, string name, string? innerText, string xmlLang)
            => Append(element, name, innerText, new() { ["xml:lang"] = xmlLang });

    private static void SerializeCode(XmlElement parent, ScriptCode scriptCode)
    {
        var e = parent.OwnerDocument.CreateElement(Elements.Code);
        _ = parent.AppendChild(e);

        foreach ((Capability capability, ScriptAction code) in scriptCode)
        {
            Append(e, capability.ResourceName, code.Code, new() { [Elements.Host] = code.Host.InvariantName });
        }
    }

    private static ScriptCode? DeserializeCodeOrDefault(XmlDocument d)
        => d.GetSingleChild(Elements.Code).ChildNodes.OfType<XmlElement>().ToDictionary(
            keySelector: e => Capability.FromResourceName(e.Name),
            elementSelector: e => new ScriptAction(Metadatas.GetMetadata<Host>(e.GetAttribute(Elements.Host)), e.InnerText))
        is { Count: > 0 } codeElements
            ? new(codeElements)
            : null;
}