using System.Text;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;

using Semver;

using static System.Globalization.CultureInfo;

using Script = Scover.WinClean.Model.Scripts.Script;

namespace Tests;

public sealed partial class ScriptXmlSerializerTests
{
    private sealed class TestScript : TestSerializable<Script>
    {
        // Elements that require additional formatting must be represented using specific types in order to
        // keep the match between the actual and expected XML
        public TestScript(string category,
                          string impact,
                              string safetyLevel,
                              LocalizedString name,
                              LocalizedString description,
                              ScriptCode code,
                              ScriptType type,
                              SemVersionRange versions) : base(
            value: new(
                category: Metadatas.GetMetadata<Category>(category),
                impact: Metadatas.GetMetadata<Impact>(impact),
                versions: versions,
                safetyLevel: Metadatas.GetMetadata<SafetyLevel>(safetyLevel),
                localizedName: name,
                localizedDescription: description,
                type: type,
                code: code),
            xml: new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>").Append(Element("Script",
                     name.FormatXml(ElementFor.Name)
                     .Append(description.FormatXml(ElementFor.Description))
                     .Append(Element(ElementFor.Category, category))
                     .Append(Element(ElementFor.SafetyLevel, safetyLevel))
                     .Append(Element(ElementFor.Impact, impact))
                     .Append(Element(ElementFor.VersionRange, versions.ToString()))
                     .Append(Element(ElementFor.Code, code.FormatXml(ElementFor.Code, (_, key, value)
                         => @$"<{key.ResourceName} {ElementFor.Host}=""{value.Host.InvariantName}"">{value.Code}</{key.ResourceName}>"))))))
        { }

        public StringBuilder ExpectedPre130Xml => !Value.Code.ContainsKey(Capability.Execute)
                   ? throw new InvalidOperationException($"Cant get expected pre 1.3.0 XML for a script that doesn't have the {Capability.Execute.ResourceName} capability")
                     : new StringBuilder($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Script>
  <Name>{Value.InvariantName}</Name>
  <Name xml:lang=""{OtherCulture.Name}"">{Value.LocalizedName[OtherCulture]}</Name>
  <Description>{Value.LocalizedDescription[InvariantCulture]}</Description>
  <Description xml:lang=""{OtherCulture.Name}"">{Value.LocalizedDescription[OtherCulture]}</Description>
  <Category>{Value.Category.InvariantName}</Category>
  <Recommended>{Value.SafetyLevel.InvariantName switch
                     {
                         "Safe" => "Yes",
                         "Dangerous" => "No",
                         _ => Value.SafetyLevel.InvariantName
                     }}</Recommended>
  <Host>{Value.Code[Capability.Execute].Host.InvariantName}</Host>
  <Impact>{Value.Impact.InvariantName}</Impact>
  <Code>{Value.Code[Capability.Execute].Code}</Code>
</Script>");
    }
}