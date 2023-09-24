using System.Reflection;
using System.Xml.Linq;

using Scover.WinClean;
using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Model.Serialization.Xml;
using Scover.WinClean.Services;

using Semver;

using static System.Globalization.CultureInfo;

namespace Tests;

[TestOf(typeof(ScriptXmlSerializer))]
public sealed partial class ScriptXmlSerializerTests
{
    private const string InvalidScriptFilesResourceNamespace = $"{nameof(Tests)}.TestScripts.Invalid";
    private const string ScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(Scover.WinClean)}.Scripts";
    private const string ValidScriptFilesResourceNamespace = $"{nameof(Tests)}.TestScripts.Valid";

    private static readonly TestCaseData[] invalidScriptFileSerializationCases = GetManifestResources(Assembly.GetExecutingAssembly(), InvalidScriptFilesResourceNamespace).Select(xml => new TestCaseData(xml)).ToArray();

    private static readonly TestCaseData[] scriptSerializationCases =
    {
        new(new Script(Metadatas.GetMetadata<Category>("Debloating"),
            new(new()
            {
                [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Cmd"), "DISM /Online /Remove-Capability /CapabilityName:Microsoft.Windows.WordPad~~~~0.0.1.0")
            }),
            Metadatas.GetMetadata<Impact>("Free storage space"),
            new LocalizedString()
            {
                [InvariantCulture] = "Remove WordPad",
                [GetCultureInfo("fr-FR")] = "Supprimer WordPad",
            },
            new LocalizedString()
            {
                [InvariantCulture] = "WordPad can be deleted if you don't use it.",
                [GetCultureInfo("fr-FR")] = "WordPad peut être supprimé si vous ne l'utilisez pas.",
            },
            Metadatas.GetMetadata<SafetyLevel>("Limited"),
            "Scover.WinClean.Scripts.RemoveWordpad.xml",
            ScriptType.Custom,
            DefaultVersions)),
        new(new Script(Metadatas.GetMetadata<Category>("Maintenance"),
            new(new()
            {
                [Capability.Enable] = new(Metadatas.GetMetadata<Host>("Regedit"), "Windows Registry Editor 5.00"),
                [Capability.Disable] = new(Metadatas.GetMetadata<Host>("Cmd"), "echo %path%"),
                [Capability.Detect] = new(Metadatas.GetMetadata<Host>("PowerShell"), "systray.exe")
            }),
            Metadatas.GetMetadata<Impact>("Privacy"),
            new LocalizedString()
            {
                [InvariantCulture] = "Test script name",
                [GetCultureInfo("fr-FR")] = "Nom du script de test",
            },
            new LocalizedString()
            {
                [InvariantCulture] ="Test script description.",
                [GetCultureInfo("fr-FR")] = "Nom du script de description.",
            },
            Metadatas.GetMetadata<SafetyLevel>("Safe"),
            @"C:\TestScript.xml",
            ScriptType.Default,
            SemVersionRange.Parse(">=10.0.1048||6.1.*"))),
        new(new Script(Metadatas.GetMetadata<Category>("Customization"),
            new(new()
            {
                [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Regedit"), @"Windows Registry Editor Version 5.00
    [HKEYCURRENTUSER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer]
    ""link""=hex:1b,00,00,00")
            }),
            Metadatas.GetMetadata<Impact>("Ergonomics"),
            new LocalizedString()
            {
                [InvariantCulture] = "Remove shortcut suffix",
                [GetCultureInfo("fr-FR")] = "Supprimer le suffixe de raccourci",
            },
            new LocalizedString
            {
                [InvariantCulture] = "By default, shortcuts are named with the suffix \"- Shortcut\".",
                [GetCultureInfo("fr-FR")] = "Par défaut, les raccourcis sont nommés avec le suffixe \" - Raccourci\".",
            },
            Metadatas.GetMetadata<SafetyLevel>("Limited"),
            "RemoveShortcutSuffix.xml",
            ScriptType.Default,
            DefaultVersions)),
    };

    private static readonly ScriptXmlSerializer serializer = new();

    private static readonly TestCaseData[] validScriptFileSerializationCases
        = GetManifestResources(Assembly.GetExecutingAssembly(), ValidScriptFilesResourceNamespace)
            .Concat(GetManifestResources(ServiceProvider.Get<IApplicationInfo>().Assembly, ScriptsResourceNamespace))
            .Select(xml => new TestCaseData(xml)).ToArray();

    private static SemVersionRange DefaultVersions => ServiceProvider.Get<ISettings>().DefaultScriptVersions;
    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    [TestCaseSource(nameof(invalidScriptFileSerializationCases))]
    public void TestInvalidScriptSerialization(string scriptFileXml) => Assert.That(() => serializer.Deserialize(scriptFileXml), Throws.InstanceOf<DeserializationException>());

    [TestCaseSource(nameof(scriptSerializationCases))]
    public void TestScriptSerialization(Script d1)
    {
        var s1 = serializer.Serialize(d1);
        var d2 = serializer.Deserialize(s1).Complete(d1.Type, d1.Source);
        var s2 = serializer.Serialize(d2);

        AssertEqual(s1, d1, s2, d2);
    }

    [TestCaseSource(nameof(validScriptFileSerializationCases))]
    public void TestValidScriptFileSerialization(string scriptFileXml)
    {
        // We can't use scriptFileXml as s1 directly as it's not exactly the same as what Serialize would output for the same script.
        // For example, it may contain comments, or the version attribute may not be specified.
        // We need to deserialize it to "sanitize" it.
        var d1 = Deserialize(scriptFileXml);

        var s1 = serializer.Serialize(d1);
        var d2 = serializer.Deserialize(s1).Complete(d1.Type, d1.Source);
        var s2 = serializer.Serialize(d2);

        static Script Deserialize(string s) => serializer.Deserialize(s).Complete(ScriptType.Default, "whateverSource"); // the completed data does not matter as it's not serialized

        AssertEqual(s1, d1, s2, d2);
    }

    private static void AssertEqual(string s1, Script d1, string s2, Script d2) => Assert.Multiple(() =>
    {
        Assert.That(XNode.DeepEquals(XDocument.Parse(s1), XDocument.Parse(s2)));
        Assert.That(d1, Is.EqualTo(d2));
    });

    private static IEnumerable<string> GetManifestResources(Assembly assembly, string resourceNamespace)
    {
        foreach (var name in assembly.GetManifestResourceNames().Where(name => name.StartsWith(resourceNamespace + '.')))
        {
            using StreamReader stream = new(assembly.GetManifestResourceStream(name).NotNull());
            yield return stream.ReadToEnd();
        }
    }
}