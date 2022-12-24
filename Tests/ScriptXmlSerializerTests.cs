using System.Globalization;
using System.Text;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation;

namespace Tests;

[TestOf(typeof(ScriptXmlSerializer))]
public sealed class ScriptXmlSerializerTests
{
    private const string
        Name = "Remove WordPad",
        NameFr = "Supprimer WordPad",
        Description = "WordPad can be deleted if you don't use it.",
        DescriptionFr = "WordPad peut être supprimé si vous ne l'utilisez pas.",
        Category = "Debloat",
        RecommendationLevel = "Limited",
        Host = "Cmd",
        Impact = "Free storage space",
        Code = "DISM /Online /Remove-Capability /CapabilityName:Microsoft.Windows.WordPad~~~~0.0.1.0",
        Xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<Script>
  <Name>{Name}</Name>
  <Name xml:lang=""fr"">{NameFr}</Name>
  <Description>{Description}</Description>
  <Description xml:lang=""fr"">{DescriptionFr}</Description>
  <Category>{Category}</Category>
  <Recommended>{RecommendationLevel}</Recommended>
  <Host>{Host}</Host>
  <Impact>{Impact}</Impact>
  <Code>{Code}</Code>
</Script>";

    private readonly IScriptSerializer serializer = new ScriptXmlSerializer(App.ScriptMetadata);

    [Test]
    public void TestDeserialize()
    {
        const string Filename = "TestCustomScript.xml";
        using (var file = File.CreateText(Filename))
        {
            file.Write(Xml);
        }
        var s = serializer.Deserialize(ScriptType.Custom, File.OpenRead(Filename));
        Assert.Multiple(() =>
        {
            Assert.That(s.LocalizedName, Is.EquivalentTo(new KeyValuePair<string, string>[] { new("", Name), new("fr", NameFr) }));
            Assert.That(s.LocalizedDescription, Is.EquivalentTo(new KeyValuePair<string, string>[] { new("", Description), new("fr", DescriptionFr) }));
            Assert.That(s.Category.InvariantName, Is.EqualTo(Category));
            Assert.That(s.RecommendationLevel.InvariantName, Is.EqualTo(RecommendationLevel));
            Assert.That(s.Host.InvariantName, Is.EqualTo(Host));
            Assert.That(s.Impact.InvariantName, Is.EqualTo(Impact));
            Assert.That(s.Code, Is.EqualTo(Code));
        });
    }

    [Test]
    public void TestSerialize()
    {
        CultureInfo fr = new("fr");
        LocalizedString name = new(), description = new();
        name.Set(CultureInfo.InvariantCulture, Name);
        name.Set(fr, NameFr);
        description.Set(CultureInfo.InvariantCulture, Description);
        description.Set(fr, DescriptionFr);

        Script s = new(localizedName: name,
                       localizedDescription: description,
                       category: App.ScriptMetadata.Get<Category>().Single(c => c.InvariantName == Category),
                       recommendationLevel: App.ScriptMetadata.Get<RecommendationLevel>().Single(r => r.InvariantName == RecommendationLevel),
                       host: App.ScriptMetadata.Get<Host>().Single(h => h.InvariantName == Host),
                       impact: App.ScriptMetadata.Get<Impact>().Single(i => i.InvariantName == Impact),
                       code: Code,
                       type: ScriptType.Custom);

        using MemoryStream ms = new();
        serializer.Serialize(s, ms);
        ms.Position = 0;
        Assert.That(new StreamReader(ms, Encoding.UTF8).ReadToEnd(), Is.EqualTo(Xml));
    }
}