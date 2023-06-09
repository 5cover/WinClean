using System.Text;
using System.Windows.Media;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization.Xml;
using Scover.WinClean.Services;

using Semver;

namespace Tests;

[TestFixture(TestOf = typeof(ScriptMetadataXmlDeserializerTests))]
public sealed partial class ScriptMetadataXmlDeserializerTests : SerializationTests
{
    private const string
        Color = "Red",
        Desc = "Desc",
        DescFr = "DescFr",
        Name = "Name",
        NameFr = "NameFr";

    private static readonly TestProgramHost host1 = new(
        Localize("Host1Name", "Host1OtherName"),
        Localize("Host1Description", "Host1OtherDescription"),
        SemVersionRange.Parse(">=6.1.0"),
        (@"%SYSTEMROOT%\System32\shell32.dll", -25),
        "Executable.exe",
        "arguments {0}",
        ".txt");

    private static readonly TestProgramHost host2 = new(
        Localize("Host2Name", "Host2OtherName"),
        Localize("Host2Description", "Host2OtherDescription"),
        DefaultVersions,
        null,
        "Executable.exe",
        "arguments {0}",
        ".txt");

    private static readonly TestShellHost host3 = new(
        Localize("Host3Name", "Host3OtherName"),
        Localize("Host3Description", "Host3OtherDescription"),
        SemVersionRange.Parse(">=6.1.0"),
        (@"%SYSTEMROOT%\System32\WindowsPowerShell\v1.0\powershell.exe", 0),
        "{0}");

    private static readonly TestShellHost host4 = new(
        Localize("Host4Name", "Host4OtherName"),
        Localize("Host4Description", "Host4OtherDescription"),
        DefaultVersions,
        null,
        "run box command line arguments {0}");

    private readonly ScriptMetadataXmlDeserializer _deserializer = new();

    private static IEnumerable<TestCaseData> CategoryCases
    {
        get
        {
            yield return new($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Category>
  <Name>{Name}</Name>
  <Name xml:lang=""fr"">{NameFr}</Name>
  <Description>{Desc}</Description>
  <Description xml:lang=""fr"">{DescFr}</Description>
</Category>", new Category(Localize(Name, NameFr), Localize(Desc, DescFr)));
        }
    }

    private static SemVersionRange DefaultVersions => ServiceProvider.Get<ISettings>().DefaultHostVersions;

    private static IEnumerable<TestCaseData> HostCases
    {
        get
        {
            yield return new(host1.Xml, host1.Value);
            yield return new(host2.Xml, host2.Value);
            yield return new(host3.Xml, host3.Value);
            yield return new(host4.Xml, host4.Value);
        }
    }

    private static IEnumerable<TestCaseData> ImpactCases
    {
        get
        {
            yield return new($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Impact>
  <Name>{Name}</Name>
  <Name xml:lang=""fr"">{NameFr}</Name>
  <Description>{Desc}</Description>
  <Description xml:lang=""fr"">{DescFr}</Description>
</Impact>", new Impact(Localize(Name, NameFr), Localize(Desc, DescFr)));
        }
    }

    private static IEnumerable<TestCaseData> SafetyLevelCases
    {
        get
        {
            yield return new TestCaseData($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<SafetyLevel Color=""{Color}"">
  <Name>{Name}</Name>
  <Name xml:lang=""fr"">{NameFr}</Name>
  <Description>{Desc}</Description>
  <Description xml:lang=""fr"">{DescFr}</Description>
</SafetyLevel>", new SafetyLevel(Localize(Name, NameFr), Localize(Desc, DescFr), (Color)ColorConverter.ConvertFromString(Color)));
        }
    }

    [TestCaseSource(nameof(CategoryCases))]
    public void TestMakeCategories(string xml, Category value)
        => Assert.That(_deserializer.GetCategories(xml.ToStream()).Single(), Is.EqualTo(value));

    [TestCaseSource(nameof(HostCases))]
    public void TestMakeHosts(StringBuilder xml, Host value)
        => Assert.That(_deserializer.GetHosts(xml.ToString().ToStream()).Single(), Is.EqualTo(value));

    [TestCaseSource(nameof(ImpactCases))]
    public void TestMakeImpacts(string xml, Impact value)
        => Assert.That(_deserializer.GetImpacts(xml.ToStream()).Single(), Is.EqualTo(value));

    [TestCaseSource(nameof(SafetyLevelCases))]
    public void TestMakeSafetyLevels(string xml, SafetyLevel value)
        => Assert.That(_deserializer.GetSafetyLevels(xml.ToStream()).Single(), Is.EqualTo(value));
}