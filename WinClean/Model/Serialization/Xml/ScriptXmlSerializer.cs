using System.Globalization;
using System.Xml;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;

using Semver;

using Script = Scover.WinClean.Model.Scripts.Script;

namespace Scover.WinClean.Model.Serialization.Xml;

public sealed class ScriptXmlSerializer : IScriptSerializer
{
    private static readonly Dictionary<string, Func<XmlDocument, ScriptBuilder>> deserializers = new()
    {
        ["Normal"] = DeserializeCurrent,
        ["Pre 1.3.0"] = DeserializePre130,
    };

    private static SemVersionRange DefaultScriptVersions => ServiceProvider.Get<ISettings>().DefaultScriptVersions;
    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    public ScriptBuilder Deserialize(Stream data) => DeserializeImpl(CreateDocument(d => d.Load(data)));

    public ScriptBuilder Deserialize(string data) => DeserializeImpl(CreateDocument(d => d.LoadXml(data)));

    public void Serialize(Script script, Stream stream)
    {
        XmlDocument d = new();

        // Explicit UTF-8 for clarity
        _ = d.AppendChild(d.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = d.CreateElement(ElementFor.Script);
        _ = d.AppendChild(root);

        // The append order is significant.

        foreach ((CultureInfo lang, string text) in script.LocalizedName)
        {
            AppendLocalizable(root, ElementFor.Name, text, lang.Name);
        }

        foreach ((CultureInfo lang, string text) in script.LocalizedDescription)
        {
            AppendLocalizable(root, ElementFor.Description, text, lang.Name);
        }

        Append(root, ElementFor.Category, script.Category.InvariantName);
        Append(root, ElementFor.SafetyLevel, script.SafetyLevel.InvariantName);
        Append(root, ElementFor.Impact, script.Impact.InvariantName);
        Append(root, ElementFor.VersionRange, script.Versions.ToString());

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

    private static ScriptCode DeserializeCode(XmlDocument d)
        => d.GetSingleChild(ElementFor.Code).ChildNodes.OfType<XmlElement>().ToDictionary(
            keySelector: e => Capability.FromResourceName(e.Name),
            elementSelector: e => new ScriptAction(Metadatas.GetMetadata<Host>(e.GetAttribute(ElementFor.Host)), e.InnerText))
        is { Count: > 0 } codeElements
            ? new ScriptCode(codeElements)
            : throw new InvalidDataException(ExceptionMessages.ElementHasNoChild.FormatWith(ElementFor.Code));

    /// <summary>Deserializes a script in the current format.</summary>
    private static ScriptBuilder DeserializeCurrent(XmlDocument d)
        => new()
        {
            Category = Metadatas.GetMetadata<Category>(d.GetSingleChildText(ElementFor.Category)),
            Code = DeserializeCode(d),
            Impact = Metadatas.GetMetadata<Impact>(d.GetSingleChildText(ElementFor.Impact)),
            LocalizedDescription = d.GetLocalizedString(ElementFor.Description),
            LocalizedName = d.GetLocalizedString(ElementFor.Name),
            SafetyLevel = Metadatas.GetMetadata<SafetyLevel>(d.GetSingleChildText(ElementFor.SafetyLevel)),
            Versions = DeserializeVersions(d),
        };

    /// <summary>Deserializes a script in the pre 1.3.0 format.</summary>
    private static ScriptBuilder DeserializePre130(XmlDocument d)
        => new()
        {
            Category = Metadatas.GetMetadata<Category>(d.GetSingleChildText(ElementFor.Category)),
            Code = new ScriptCode(new()
            {
                [Capability.Execute] = new ScriptAction(
                    host: Metadatas.GetMetadata<Host>(d.GetSingleChildText(ElementFor.Host)),
                        code: d.GetSingleChildText(ElementFor.Code))
            }),
            Impact = Metadatas.GetMetadata<Impact>(d.GetSingleChildText(ElementFor.Impact) switch
            {
                var s when s.Equals("Debloat", Metadatas.Comparison) => "Debloating",
                var s => s
            }),
            LocalizedDescription = d.GetLocalizedString(ElementFor.Description),
            LocalizedName = d.GetLocalizedString(ElementFor.Name),
            SafetyLevel = Metadatas.GetMetadata<SafetyLevel>(d.GetSingleChildText("Recommended") switch
            {
                var s when s.Equals("Yes", Metadatas.Comparison) => "Safe",
                var s when s.Equals("No", Metadatas.Comparison) => "Dangerous",
                var s => s,
            }),
            Versions = DefaultScriptVersions,
        };

    private static SemVersionRange DeserializeVersions(XmlDocument d)
        => d.GetSingleChildTextOrDefault(ElementFor.VersionRange) is { } versionsStr
            ? SemVersionRange.Parse(versionsStr)
            : DefaultScriptVersions;

    private static void SerializeCode(XmlElement parent, ScriptCode scriptCode)
    {
        var e = parent.OwnerDocument.CreateElement(ElementFor.Code);
        _ = parent.AppendChild(e);

        foreach ((Capability capability, ScriptAction code) in scriptCode)
        {
            Append(e, capability.ResourceName, code.Code, new() { [ElementFor.Host] = code.Host.InvariantName });
        }
    }

    private XmlDocument CreateDocument(Action<XmlDocument> load)
    {
        XmlDocument d = new();
        try
        {
            load(d);
        }
        catch (XmlException e)
        {
            throw new DeserializationException(nameof(Script), innerException: e);
        }
        return d;
    }

    private ScriptBuilder DeserializeImpl(XmlDocument loadedDocument)
    {
        Dictionary<string, Exception> deserializerExceptions = new();

        foreach ((var name, var deserializer) in deserializers)
        {
            try
            {
                return deserializer(loadedDocument);
            }
            catch (Exception e) when (e is InvalidOperationException or XmlException or KeyNotFoundException or FormatException or InvalidDataException)
            {
                deserializerExceptions[name] = e;
            }
        }

        throw new DeserializationChainException(nameof(Script), loadedDocument.OuterXml, deserializerExceptions);
    }
}