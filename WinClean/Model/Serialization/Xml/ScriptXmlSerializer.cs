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
    private static readonly int[] defaultSuccessExitCodes = { 0 };

    private static readonly Dictionary<string, Func<XmlDocument, ScriptBuilder>> deserializers = new()
    {
        ["Normal"] = DeserializeCurrent,
        ["Pre 1.3.0"] = DeserializePre130,
    };

    private static SemVersionRange DefaultScriptVersions => ServiceProvider.Get<ISettings>().DefaultScriptVersions;
    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    public ScriptBuilder Deserialize(Stream data) => DeserializeImpl(CreateDocument(d => d.Load(data)));

    public ScriptBuilder Deserialize(string data) => DeserializeImpl(CreateDocument(d => d.LoadXml(data)));

    public void Serialize(Script script, Stream stream) => SerializeImpl(script).Save(stream);

    public string Serialize(Script script) => SerializeImpl(script).OuterXml;

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
        => d.GetSingleChild(NameFor.Code).ChildNodes.OfType<XmlElement>().ToDictionary(
            keySelector: e => Capability.FromResourceName(e.Name),
            elementSelector: e => new ScriptAction(Metadatas.GetMetadata<Host>(e.GetAttribute(NameFor.Host)),
                e.GetAttribute(NameFor.SuccessExitCodes).Split(' ', StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } successExitCodes
                ? successExitCodes.Select(s => int.Parse(s, CultureInfo.InvariantCulture))
                : defaultSuccessExitCodes, e.InnerText))
        is { Count: > 0 } codeElements
            ? new ScriptCode(codeElements)
            : throw new InvalidDataException(ExceptionMessages.ElementHasNoNamedChild.FormatWith(NameFor.Code));

    /// <summary>Deserializes a script in the current format.</summary>
    private static ScriptBuilder DeserializeCurrent(XmlDocument d)
        => new()
        {
            Category = Metadatas.GetMetadata<Category>(d.GetSingleChildText(NameFor.Category)),
            Code = DeserializeCode(d),
            Impact = Metadatas.GetMetadata<Impact>(d.GetSingleChildText(NameFor.Impact)),
            LocalizedDescription = d.GetLocalizedString(NameFor.Description),
            LocalizedName = d.GetLocalizedString(NameFor.Name),
            SafetyLevel = Metadatas.GetMetadata<SafetyLevel>(d.GetSingleChildText(NameFor.SafetyLevel)),
            Versions = DeserializeVersions(d),
        };

    private static ScriptBuilder DeserializeImpl(XmlDocument loadedDocument)
    {
        Dictionary<string, Exception> deserializerExceptions = new();

        foreach ((var name, var deserializer) in deserializers)
        {
            try
            {
                return deserializer(loadedDocument);
            }
            catch (Exception e) when (e is InvalidOperationException or XmlException or KeyNotFoundException or FormatException or InvalidDataException or OverflowException)
            {
                deserializerExceptions[name] = e;
            }
        }

        throw new DeserializationChainException(nameof(Script), loadedDocument.OuterXml, deserializerExceptions);
    }

    /// <summary>Deserializes a script in the pre 1.3.0 format.</summary>
    private static ScriptBuilder DeserializePre130(XmlDocument d)
        => new()
        {
            Category = Metadatas.GetMetadata<Category>(d.GetSingleChildText(NameFor.Category)),
            Code = new ScriptCode(new()
            {
                [Capability.Execute] = new ScriptAction(
                    code: d.GetSingleChildText(NameFor.Code),
                    successsExitCodes: defaultSuccessExitCodes,
                    host: Metadatas.GetMetadata<Host>(d.GetSingleChildText(NameFor.Host)))
            }),
            Impact = Metadatas.GetMetadata<Impact>(d.GetSingleChildText(NameFor.Impact) switch
            {
                var s when s.Equals("Debloat", Metadatas.Comparison) => "Debloating",
                var s => s
            }),
            LocalizedDescription = d.GetLocalizedString(NameFor.Description),
            LocalizedName = d.GetLocalizedString(NameFor.Name),
            SafetyLevel = Metadatas.GetMetadata<SafetyLevel>(d.GetSingleChildText("Recommended") switch
            {
                var s when s.Equals("Yes", Metadatas.Comparison) => "Safe",
                var s when s.Equals("No", Metadatas.Comparison) => "Dangerous",
                var s => s,
            }),
            Versions = DefaultScriptVersions,
        };

    private static SemVersionRange DeserializeVersions(XmlDocument d)
        => d.GetSingleChildTextOrDefault(NameFor.VersionRange) is { } versionsStr
            ? SemVersionRange.Parse(versionsStr)
            : DefaultScriptVersions;

    private static void SerializeCode(XmlElement parent, ScriptCode scriptCode)
    {
        var e = parent.OwnerDocument.CreateElement(NameFor.Code);
        _ = parent.AppendChild(e);

        foreach ((Capability capability, ScriptAction code) in scriptCode)
        {
            Append(e, capability.ResourceName, code.Code, new() { [NameFor.Host] = code.Host.InvariantName });
        }
    }

    private static XmlDocument SerializeImpl(Script script)
    {
        XmlDocument d = new();

        // Explicit UTF-8 for clarity
        _ = d.AppendChild(d.CreateXmlDeclaration("1.0", "UTF-8", null));

        var root = d.CreateElement(NameFor.Script);
        _ = d.AppendChild(root);

        // The append order is significant.

        foreach ((CultureInfo lang, string text) in script.LocalizedName)
        {
            AppendLocalizable(root, NameFor.Name, text, lang.Name);
        }

        foreach ((CultureInfo lang, string text) in script.LocalizedDescription)
        {
            AppendLocalizable(root, NameFor.Description, text, lang.Name);
        }

        Append(root, NameFor.Category, script.Category.InvariantName);
        Append(root, NameFor.SafetyLevel, script.SafetyLevel.InvariantName);
        Append(root, NameFor.Impact, script.Impact.InvariantName);
        Append(root, NameFor.VersionRange, script.Versions.ToString());

        SerializeCode(root, script.Code);

        return d;
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
}