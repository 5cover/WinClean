using System.Globalization;
using System.Windows.Media;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;

namespace Tests;

[TestFixture(TestOf = typeof(ScriptMetadataXmlDeserializerTests))]
public sealed class ScriptMetadataXmlDeserializerTests
{
    private const string
        Name = "Name",
        NameEn = "NameEn",
        NameFr = "NameFr",
        Desc = "Desc",
        DescFr = "DescFr",
        Executable = "Executable.exe",
        Arguments = "arguments {0}",
        Extension = ".txt",
        Color = "Red";

    private static readonly CultureInfo cultureEn = new("en");
    private static readonly CultureInfo cultureFr = new("fr");
    private readonly ScriptMetadataXmlDeserializer _deserializer = new();

    [TestCase($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Categories>
  <Category>
    <Name>{Name}</Name>
    <Name xml:lang=""fr"">{NameFr}</Name>
    <Description>{Desc}</Description>
    <Description xml:lang=""fr"">{DescFr}</Description>
  </Category>
</Categories>")]
    public void TestMakeCategories(string xml)
        => Assert.That(_deserializer.MakeCategories(xml.ToStream()).Single(), Is.EqualTo(new Category(Loc(Name, NameFr), Loc(Desc, DescFr))));

    [TestCase($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Hosts>
  <Host>
    <Name>{Name}</Name>
    <Name xml:lang=""en"">{NameEn}</Name>
    <Name xml:lang=""fr"">{NameFr}</Name>
    <Description>{Desc}</Description>
    <Description xml:lang=""fr"">{DescFr}</Description>
    <Executable>{Executable}</Executable>
    <Arguments>{Arguments}</Arguments>
    <Extension>{Extension}</Extension>
  </Host>
</Hosts>")]
    public void TestMakeHosts(string xml)
        => Assert.That(_deserializer.MakeHosts(xml.ToStream()).Single(), Is.EqualTo(new Host(Loc(Name, NameFr, NameEn), Loc(Desc, DescFr), Executable, Arguments, Extension)));

    [TestCase($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Impacts>
  <Impact>
    <Name>{Name}</Name>
    <Name xml:lang=""fr"">{NameFr}</Name>
    <Description>{Desc}</Description>
    <Description xml:lang=""fr"">{DescFr}</Description>
  </Impact>
</Impacts>")]
    public void TestMakeImpacts(string xml)
        => Assert.That(_deserializer.MakeImpacts(xml.ToStream()).Single(), Is.EqualTo(new Impact(Loc(Name, NameFr), Loc(Desc, DescFr))));

    [TestCase($@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<RecommendationLevels>
  <RecommendationLevel Color=""{Color}"">
    <Name>{Name}</Name>
    <Name xml:lang=""fr"">{NameFr}</Name>
    <Description>{Desc}</Description>
    <Description xml:lang=""fr"">{DescFr}</Description>
  </RecommendationLevel>
</RecommendationLevels>")]
    public void TestMakeRecommendationLevels(string xml)
        => Assert.That(_deserializer.MakeRecommendationLevels(xml.ToStream()).Single(), Is.EqualTo(new RecommendationLevel(Loc(Name, NameFr), Loc(Desc, DescFr), (Color)ColorConverter.ConvertFromString(Color))));

    private static LocalizedString Loc(string invariant, string fr)
    {
        LocalizedString ls = new();
        ls.Set(CultureInfo.InvariantCulture, invariant);
        ls.Set(ScriptMetadataXmlDeserializerTests.cultureFr, fr);
        return ls;
    }

    private static LocalizedString Loc(string invariant, string fr, string en)
    {
        LocalizedString ls = Loc(invariant, fr);
        ls.Set(ScriptMetadataXmlDeserializerTests.cultureEn, en);
        return ls;
    }
}