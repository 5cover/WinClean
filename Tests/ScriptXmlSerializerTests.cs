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
public sealed class ScriptXmlSerializerTests
{
    private const string InvalidScriptFilesResourceNamespace = $"{nameof(Tests)}.TestScripts.Invalid";
    private const string ScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(Scover.WinClean)}.Scripts";
    private const string ValidScriptFilesResourceNamespace = $"{nameof(Tests)}.TestScripts.Valid";

    private static readonly TestCaseData[] invalidScriptFileSerializationCases = GetManifestResources(Assembly.GetExecutingAssembly(), InvalidScriptFilesResourceNamespace).Select(xml => new TestCaseData(xml)).ToArray();

    private static readonly TestCaseData[] scriptSerializationCases =
    {
        new(new Script(new Dictionary<Capability, ScriptAction>
        {
            [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Cmd"), new HashSet<int> { 0, 3010 }, "DISM /Online /Remove-Capability /CapabilityName:Microsoft.Windows.WordPad~~~~0.0.1.0 /NoRestart", 1),
        },
        Metadatas.GetMetadata<Category>("Debloating"),
        Metadatas.GetMetadata<Impact>("Free storage space"),
        new LocalizedString
        {
            [InvariantCulture] = "Remove WordPad",
            [GetCultureInfo("fr-FR")] = "Supprimer WordPad",
        },
        new LocalizedString
        {
            [InvariantCulture] = "WordPad can be deleted if you don't use it.",
            [GetCultureInfo("fr-FR")] = "WordPad peut être supprimé si vous ne l'utilisez pas.",
        },
        Metadatas.GetMetadata<SafetyLevel>("Limited"),
        "Scover.WinClean.Scripts.RemoveWordpad.xml",
        ScriptType.Custom,
        DefaultVersions)),

        new(new Script(new Dictionary<Capability, ScriptAction>
        {
            [Capability.Enable] = new(Metadatas.GetMetadata<Host>("Regedit"), new HashSet<int> { 0 }, "Windows Registry Editor 5.00", 0),
            [Capability.Disable] = new(Metadatas.GetMetadata<Host>("Cmd"), new HashSet<int> { 0 }, "echo %path%", 0),
            [Capability.Detect] = new(Metadatas.GetMetadata<Host>("PowerShell"), new HashSet<int> { 0 }, "systray.exe", 0),
        },
        Metadatas.GetMetadata<Category>("Maintenance"),
        Metadatas.GetMetadata<Impact>("Privacy"),
        new LocalizedString
        {
            [InvariantCulture] = "Test script name",
            [GetCultureInfo("fr-FR")] = "Nom du script de test",
        },
        new LocalizedString
        {
            [InvariantCulture] = "Test script description.",
            [GetCultureInfo("fr-FR")] = "Nom du script de description.",
        },
        Metadatas.GetMetadata<SafetyLevel>("Safe"),
        @"C:\TestScript.xml",
        ScriptType.Default,
        SemVersionRange.Parse(">=10.0.1048||6.1.*"))),

        new(new Script(new Dictionary<Capability, ScriptAction>
        {
            [Capability.Execute] = new(Metadatas.GetMetadata<Host>("Regedit"), new HashSet<int> { 0 }, @"Windows Registry Editor Version 5.00
[HKEYCURRENTUSER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer]
""link""=hex:1b,00,00,00", 0),
        },
        Metadatas.GetMetadata<Category>("Customization"),
        Metadatas.GetMetadata<Impact>("Ergonomics"),
        new LocalizedString
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

    private static readonly IScriptSerializer serializer = new ScriptXmlSerializer();

    private static readonly TestCaseData[] validScriptFileSerializationCases
        = GetManifestResources(Assembly.GetExecutingAssembly(), ValidScriptFilesResourceNamespace)
            .Concat(GetManifestResources(ServiceProvider.Get<IApplicationInfo>().Assembly, ScriptsResourceNamespace))
            .Select(xml => new TestCaseData(xml)).ToArray();

    private static SemVersionRange DefaultVersions => ServiceProvider.Get<ISettings>().DefaultScriptVersions;
    private static IMetadatasProvider Metadatas => ServiceProvider.Get<IMetadatasProvider>();

    // Round-trip serialization tests

    [TestCaseSource(nameof(invalidScriptFileSerializationCases))]
    public void TestInvalidScriptSerialization(string scriptFileXml) => Assert.That(() => serializer.Deserialize(scriptFileXml), Throws.InstanceOf<DeserializationException>());

    [TestCaseSource(nameof(scriptSerializationCases))]
    public void TestScriptSerialization(Script d1)
    {
        var s1 = serializer.Serialize(d1);
        var d2 = serializer.Deserialize(s1).Complete(d1.Type, d1.Source);
        var s2 = serializer.Serialize(d2);

        Assert.Multiple(() =>
        {
            Assert.That(XNode.DeepEquals(XDocument.Parse(s1), XDocument.Parse(s2)));
            //Assert.That(ItemsEqual(d1.Actions, d2.Actions, ActionsComparer.Instance), Is.True);
            Assert.That(d1.Actions, Is.EquivalentTo(d2.Actions).Using(ActionsComparer.Instance));
            Assert.That(d1.Category, Is.EqualTo(d2.Category));
            Assert.That(d1.Impact, Is.EqualTo(d2.Impact));
            Assert.That(d1.LocalizedDescription, Is.EqualTo(d2.LocalizedDescription).Using(LocalizedStringComparer.Instance));
            Assert.That(d1.LocalizedName, Is.EqualTo(d2.LocalizedName).Using(LocalizedStringComparer.Instance));
            Assert.That(d1.SafetyLevel, Is.EqualTo(d2.SafetyLevel));
            Assert.That(d1.Source, Is.EqualTo(d2.Source));
            Assert.That(d1.Type, Is.EqualTo(d2.Type));
            Assert.That(d1.Versions, Is.EqualTo(d2.Versions));
        });
    }

    [TestCaseSource(nameof(validScriptFileSerializationCases))]
    public void TestValidScriptFileSerialization(string scriptFileXml)
    {
        // We can't use scriptFileXml as s1 directly as it's not exactly the same as what Serialize would output for the same script.
        // For example, it may contain comments, or the version attribute may not be specified.
        // We need to deserialize it to "sanitize" it.
        var d1 = serializer.Deserialize(scriptFileXml).Complete(ScriptType.Default, "whateverSource"); // the completed data does not matter as it's not serialized

        TestScriptSerialization(d1);
    }

    private static IEnumerable<string> GetManifestResources(Assembly assembly, string resourceNamespace)
    {
        foreach (var name in assembly.GetManifestResourceNames().Where(name => name.StartsWith(resourceNamespace + '.', StringComparison.Ordinal)))
        {
            using StreamReader stream = new(assembly.GetManifestResourceStream(name).NotNull());
            yield return stream.ReadToEnd();
        }
    }

    // Unlique with LINQ SequenceEqual, the order of the elements isn't taken into account.
    private static bool ItemsEqual<T>(IEnumerable<T> e1, IEnumerable<T> e2, IEqualityComparer<T>? comparer = null) => !e1.Except(e2, comparer).Any();

    private sealed class ActionsComparer : IEqualityComparer<KeyValuePair<Capability, ScriptAction>>
    {
        private ActionsComparer()
        {
        }

        public static ActionsComparer Instance { get; } = new();

        public bool Equals(KeyValuePair<Capability, ScriptAction> x, KeyValuePair<Capability, ScriptAction> y)
            => x.Key.Equals(y.Key)
            && x.Value.Code == y.Value.Code
            && x.Value.Host.Equals(y.Value.Host)
            && x.Value.SuccessExitCodes.SetEquals(y.Value.SuccessExitCodes)
            && x.Value.Order == y.Value.Order;

        public int GetHashCode(KeyValuePair<Capability, ScriptAction> obj) => obj.GetHashCode();
    }

    private sealed class LocalizedStringComparer : IEqualityComparer<LocalizedString>
    {
        private LocalizedStringComparer()
        {
        }

        public static LocalizedStringComparer Instance { get; } = new();

        public bool Equals(LocalizedString? x, LocalizedString? y)
            => ReferenceEquals(x, y) || x is not null && y is not null && ItemsEqual(x, y);

        public int GetHashCode(LocalizedString obj) => obj.GetHashCode();
    }
}