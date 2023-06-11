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
        public TestScript(Script s) : base(s,
            new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>").Append(Element(ElementFor.Script,
                s.LocalizedName.FormatXml(ElementFor.Name)
                .Append(s.LocalizedDescription.FormatXml(ElementFor.Description))
                .Append(Element(ElementFor.Category, s.Category.InvariantName))
                .Append(Element(ElementFor.SafetyLevel, s.SafetyLevel.InvariantName))
                .Append(Element(ElementFor.Impact, s.Impact.InvariantName))
                .Append(Element(ElementFor.VersionRange, s.Versions.ToString()))
                .Append(Element(ElementFor.Code, s.Code.FormatXml(ElementFor.Code, (_, key, value)
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

        public static TestScript Create(string category,
                                 string impact,
                                 string safetyLevel,
                                 LocalizedString name,
                                 LocalizedString description,
                                 ScriptCode code,
                                 ScriptType type,
                                 SemVersionRange versions) => new(new(
               category: Metadatas.GetMetadata<Category>(category),
               impact: Metadatas.GetMetadata<Impact>(impact),
               versions: versions,
               safetyLevel: Metadatas.GetMetadata<SafetyLevel>(safetyLevel),
               localizedName: name,
               localizedDescription: description,
               type: type,
               code: code));
    }
}