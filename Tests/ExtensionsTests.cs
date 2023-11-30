using System.Globalization;
using System.Xml;

using Humanizer;

using Scover.WinClean;
using Scover.WinClean.Model;
using Scover.WinClean.Resources;

namespace Tests;

[TestOf(typeof(Extensions))]
public sealed class ExtensionsTests
{
    private static readonly TestCaseData[] getSingleChildExceptionCases =
    {
        new("<Tests/>", "Test", ExceptionMessages.ElementHasNoNamedChild.FormatWith("Tests", "Test")),
        new("<Tests><Test/><Test/></Tests>", "Test", ExceptionMessages.ElementHasMultipleNamedChilds.FormatWith("Tests", "Test", 2)),
    };

    private static readonly TestCaseData[] sumCases =
    {
        new(10.Hours(), new[] { 10.Hours() }),
        new(10.Hours(), new[] { 5.Hours(), 5.Hours() }),
        new(10.Hours(), new[] { 5, 3, 2 }.Select(s => s.Hours())),
        new(10.Hours(), new[] { 0.3, 1.7, 4.6, 3.4 }.Select(s => s.Hours())),
    };

    [TestCase("Test", "value", "<Tests><Test>value</Test></Tests>")]
    public void TestGetSingleChild(string name, string innerText, string xml)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);

        Assert.That(doc.GetSingleChildText(name), Is.EqualTo(innerText));
    }

    [TestCaseSource(nameof(getSingleChildExceptionCases))]
    public void TestGetSingleChildException(string xml, string elementName, string exceptionMessage)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);

        var e = Assert.Throws<XmlException>(() => doc.GetSingleChildText(elementName));
        Assert.That(e.NotNull().Message, Is.EqualTo(exceptionMessage));
    }

    [TestCase("<Test>value</Test>", "", "value")]
    [TestCase("<Test xml:lang=\"fr\">valueFr</Test>", "fr", "valueFr")]
    [TestCase("<TestElement/>", "", "")]
    [TestCase("<TestElement xml:lang=\"fr\"/>", "fr", "")]
    [TestCase("<TestElement xml:lang=\"en-US\"/>", "en-US", "")]
    public void TestSetFromXml(string xml, string cultureName, string value)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);
        LocalizedString str = new();

        str.SetFromXml(doc.DocumentElement.NotNull());

        Assert.That(str.Single(), Is.EqualTo(KeyValuePair.Create(new CultureInfo(cultureName), value)));
    }

    [TestCaseSource(nameof(sumCases))]
    public void TestSum(TimeSpan total, IEnumerable<TimeSpan> elements)
        => Assert.That(total, Is.EqualTo(elements.Sum(ts => ts)));

    [TestCase(new object[] { new object[0] })]
    [TestCase(new object[] { new object?[] { null } })]
    [TestCase(new object[] { new object?[] { null, null, null } })]
    [TestCase(new object[] { new object?[] { null, "", 0, 10.9, null } })]
    public void TestWithoutNull(IEnumerable<object?> sequence)
    {
        var seq = sequence.ToList();
        var withoutNull = seq.WithoutNull().ToList();
        Assert.That(withoutNull, Does.Not.Contains(null));
        Assert.That(withoutNull, Has.Count.EqualTo(seq.Count - seq.Count(o => o is null)));
    }

    [TestCase("1.2.3")]
    [TestCase("1.2.3.4")]
    public void TestWithoutRevision(string version) => Assert.That(new Version(version).WithoutRevision().Revision, Is.EqualTo(-1));
}