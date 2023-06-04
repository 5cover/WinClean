using System.Collections;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization.Xml;
using Scover.WinClean.Services;

using Semver;

using static System.Globalization.CultureInfo;

namespace Tests;

[TestOf(typeof(ScriptXmlSerializer))]
public sealed partial class ScriptXmlSerializerTests
{
    private const string otherCultureName = "fr";
    private static readonly CultureInfo otherCulture = new(otherCultureName);
    private static SemVersionRange DefaultVersions => ServiceProvider.Get<ISettings>().DefaultScriptSupportedVersionRange;
    private static readonly TestingScript script1 = new("Debloat", "Free storage space", "Limited",
        new()
        {
            [InvariantCulture] = "Remove WordPad",
            [otherCulture] = "Supprimer WordPad"
        },
        new()
        {
            [InvariantCulture] = "WordPad can be deleted if you don't use it.",
            [otherCulture] = "WordPad peut être supprimé si vous ne l'utilisez pas."
        },
        new()
        {
            [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Cmd"), "DISM /Online /Remove-Capability /CapabilityName:Microsoft.Windows.WordPad~~~~0.0.1.0")
        },
        ScriptType.Custom,
        DefaultVersions
        );

    private static readonly TestingScript script2 = new("Maintenance", "Privacy", "Safe",
        new()
        {
            [InvariantCulture] = "Test script name",
            [otherCulture] = "Nom du script de test"
        },
        new()
        {
            [InvariantCulture] = "Test script description.",
            [otherCulture] = "Nom du script de description."
        },
        new()
        {
            [Capability.Enable] = new(Metadatas.GetMetadata<Host>("Regedit"), "Windows Registry Editor 5.00"),
            [Capability.Disable] = new(Metadatas.GetMetadata<Host>("Cmd"), "echo %path%"),
            [Capability.Detect] = new(Metadatas.GetMetadata<Host>("PowerShell"), "systray.exe")
        },
        ScriptType.Default,
        SemVersionRange.Parse(">=10.0.1048||6.1.*"));

    private static readonly TestingScript script3 = new("Customization", "Ergonomics", "Limited",
        new()
        {
            [InvariantCulture] = "Remove shortcut suffix",
            [otherCulture] = "Supprimer le suffixe de raccourci",
        },
        new()
        {
            [InvariantCulture] = "By default, shortcuts are named with the suffix \"- Shortcut\".",
            [otherCulture] = "Par défaut, les raccourcis sont nommés avec le suffixe \" - Raccourci\".",
        },
        new()
        {
            [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Regedit"), @"Windows Registry Editor Version 5.00
[HKEYCURRENTUSER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer]
""link""=hex:1b,00,00,00")
        },
        ScriptType.Default,
        DefaultVersions);

    private readonly ScriptXmlSerializer _serializer = new(DefaultVersions);
    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    [TestCaseSource(typeof(DeserializeCases))]
    public void TestDeserialize(StringBuilder xml, Script expected)
    {
        const string Filename = "TestCustomScript.xml";

        using (var file = File.CreateText(Filename))
        {
            file.Write(xml);
        }

        Script s;
        using (var file = File.OpenRead(Filename))
        {
            s = _serializer.Deserialize(expected.Type, file);
        }

        AssertScriptsEqual(s, expected);
    }

    [TestCaseSource(typeof(SerializeCases))]
    public void TestSerialize(Script script, StringBuilder expectedXml)
    {
        using MemoryStream ms = new();
        _serializer.Serialize(script, ms);
        ms.Position = 0;
        Assert.That(XNode.DeepEquals(XElement.Load(ms), XElement.Parse(expectedXml.ToString())));
    }

    private static void AssertScriptsEqual(Script script1, Script script2) => Assert.Multiple(() =>
    {
        Assert.That(script1.Category, Is.EqualTo(script2.Category));
        Assert.That(script1.Impact, Is.EqualTo(script2.Impact));
        Assert.That(script1.LocalizedDescription, Is.EqualTo(script2.LocalizedDescription));
        Assert.That(script1.LocalizedName, Is.EqualTo(script2.LocalizedName));
        Assert.That(script1.SafetyLevel, Is.EqualTo(script2.SafetyLevel));
        Assert.That(script1.Type, Is.EqualTo(script2.Type));
        Assert.That(script1.Code, Is.EquivalentTo(script2.Code));
    });

    private sealed class DeserializeCases : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            yield return new(script1.ExpectedXml, script1.Script);
            yield return new(script2.ExpectedXml, script2.Script);
            yield return new(script3.ExpectedXml, script3.Script);
            yield return new(script3.ExpectedPre130Xml, script3.Script);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class SerializeCases : IEnumerable<TestCaseData>
    {
        public IEnumerator<TestCaseData> GetEnumerator()
        {
            yield return new(script1.Script, script1.ExpectedXml);
            yield return new(script2.Script, script2.ExpectedXml);
            yield return new(script3.Script, script3.ExpectedXml);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}