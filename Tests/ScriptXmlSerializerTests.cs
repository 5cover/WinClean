using System.Text;
using System.Xml.Linq;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization.Xml;
using Scover.WinClean.Services;

using Semver;

namespace Tests;

[TestOf(typeof(ScriptXmlSerializer))]
public sealed partial class ScriptXmlSerializerTests : SerializationTests
{
    private const string ScriptResourceNamespace = $"{nameof(Scover)}.{nameof(Scover.WinClean)}.Scripts.";

    private static readonly TestScript script1 = TestScript.Create("Debloating", "Free storage space", "Limited",
        Localize("Remove WordPad", "Supprimer WordPad"),
        Localize("WordPad can be deleted if you don't use it.", "WordPad peut être supprimé si vous ne l'utilisez pas."),
        new(new()
        {
            [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Cmd"), "DISM /Online /Remove-Capability /CapabilityName:Microsoft.Windows.WordPad~~~~0.0.1.0")
        }),
        ScriptType.Custom,
        DefaultVersions,
        "Scover.WinClean.Scripts.RemoveWordpad.xml");

    private static readonly TestScript script2 = TestScript.Create("Maintenance", "Privacy", "Safe",
        Localize("Test script name", "Nom du script de test"),
        Localize("Test script description.", "Nom du script de description."),
        new(new()
        {
            [Capability.Enable] = new(Metadatas.GetMetadata<Host>("Regedit"), "Windows Registry Editor 5.00"),
            [Capability.Disable] = new(Metadatas.GetMetadata<Host>("Cmd"), "echo %path%"),
            [Capability.Detect] = new(Metadatas.GetMetadata<Host>("PowerShell"), "systray.exe")
        }),
        ScriptType.Default,
        SemVersionRange.Parse(">=10.0.1048||6.1.*"),
        @"C:\TestScript.xml");

    private static readonly TestScript script3 = TestScript.Create("Customization", "Ergonomics", "Limited",
        Localize("Remove shortcut suffix", "Supprimer le suffixe de raccourci"),
        Localize("By default, shortcuts are named with the suffix \"- Shortcut\".", "Par défaut, les raccourcis sont nommés avec le suffixe \" - Raccourci\"."),
        new(new()
        {
            [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Regedit"), @"Windows Registry Editor Version 5.00
[HKEYCURRENTUSER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer]
""link""=hex:1b,00,00,00")
        }),
        ScriptType.Default,
        DefaultVersions,
        "RemoveShortcutSuffix.xml");

    private static readonly ScriptXmlSerializer serializer = new();
    private static SemVersionRange DefaultVersions => ServiceProvider.Get<ISettings>().DefaultScriptVersions;

    private static IEnumerable<TestCaseData> DeserializeCases
    {
        get
        {
            yield return new(script1.Xml, script1.Value);
            yield return new(script2.Xml, script2.Value);
            yield return new(script3.Xml, script3.Value);
            yield return new(script3.ExpectedPre130Xml, script3.Value);
        }
    }

    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    private static IEnumerable<TestCaseData> SerializeCases
    {
        get
        {
            yield return new(script1.Value, script1.Xml);
            yield return new(script2.Value, script2.Xml);
            yield return new(script3.Value, script3.Xml);
        }
    }

    [TestCaseSource(nameof(DeserializeCases))]
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
            s = serializer.Deserialize(file).Complete(expected.Type, expected.Source);
        }

        AssertScriptsEqual(s, expected);
    }

    [TestCaseSource(nameof(SerializeCases))]
    public void TestSerialize(Script script, StringBuilder expectedXml)
    {
        using MemoryStream ms = new();
        serializer.Serialize(script, ms);
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
        Assert.That(script1.Source, Is.EquivalentTo(script2.Source));
    });
}