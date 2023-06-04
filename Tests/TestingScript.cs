using System.Globalization;
using System.Text;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Services;

using Semver;

using static System.Globalization.CultureInfo;
using static Scover.WinClean.Model.Serialization.Xml.ScriptXmlSerializer;

namespace Tests;

public sealed partial class ScriptXmlSerializerTests
{
    private sealed class TestingScript
    {
        // Elements that require additional formatting must be represented using specific types in order to
        // keep the match between the actual and expected XML
        public TestingScript(string category, string impact, string safetyLevel, Dictionary<CultureInfo, string> name, Dictionary<CultureInfo, string> description, Dictionary<Capability, ScriptAction> code, ScriptType type, SemVersionRange versions)
        {
            var defaultVersions = ServiceProvider.Get<ISettings>().DefaultScriptSupportedVersionRange;
            Script = new(
                category: Metadatas.GetMetadata<Category>(category),
                impact: Metadatas.GetMetadata<Impact>(impact),
                versions: versions,
                safetyLevel: Metadatas.GetMetadata<SafetyLevel>(safetyLevel),
                localizedName: new(name),
                localizedDescription: new(description),
                type: type,
                code: new(code));

            ExpectedXml = new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?><Script>")
                .Append(name.Aggregate(new StringBuilder(), (sum, value) => sum.Append(FormatLocalized(Elements.Name, value.Key, value.Value)), sum => sum.ToString()))
                .Append(description.Aggregate(new StringBuilder(), (sum, value) => sum.Append(FormatLocalized(Elements.Description, value.Key, value.Value)), sum => sum.ToString()))
                .Append(Element(Elements.Category, category))
                .Append(Element(Elements.Safety, safetyLevel))
                .Append(Element(Elements.Impact, impact))
                .Append(Element(Elements.Versions, versions.ToString()))
                .Append(Element(Elements.Code, code.Aggregate(new StringBuilder(), (sum, value) => sum.Append(FormatCode(value.Key, value.Value)), sum => sum.ToString())))
                .Append("</Script>");
        }

        public StringBuilder ExpectedPre130Xml => !Script.Code.ContainsKey(Capability.Execute)
                    ? throw new InvalidOperationException($"Cant get expected pre 1.3.0 XML for a script that doesn't have the {Capability.Execute.ResourceName} capability")
                    : new StringBuilder($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Script>
  <Name>{Script.Name}</Name>
  <Name xml:lang=""{otherCultureName}"">{Script.LocalizedName[otherCulture]}</Name>
  <Description>{Script.LocalizedDescription[InvariantCulture]}</Description>
  <Description xml:lang=""{otherCultureName}"">{Script.LocalizedDescription[otherCulture]}</Description>
  <Category>{Script.Category.InvariantName}</Category>
  <Recommended>{Script.SafetyLevel.InvariantName switch
                    {
                        "Safe" => "Yes",
                        "Dangerous" => "No",
                        _ => Script.SafetyLevel.InvariantName
                    }}</Recommended>
  <Host>{Script.Code[Capability.Execute].Host.InvariantName}</Host>
  <Impact>{Script.Impact.InvariantName}</Impact>
  <Code>{Script.Code[Capability.Execute].Code}</Code>
</Script>");

        public StringBuilder ExpectedXml { get; }
        public Script Script { get; }

        private static string Element(string name, string contents) => $"<{name}>{contents}</{name}>";

        private static string FormatCode(Capability capability, ScriptAction code)
                    => @$"<{capability.ResourceName} {Elements.Host}=""{code.Host.InvariantName}"">{code.Code}</{capability.ResourceName}>";

        private static string FormatLocalized(string elementName, CultureInfo lang, string value) => $@"<{elementName} xml:lang=""{lang.Name}"">{value}</{elementName}>";
    }
}