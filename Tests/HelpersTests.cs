using System.Collections;
using System.Xml;
using Humanizer;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation;

namespace Tests;

public sealed class HelpersTests
{
    private const string ToFilenameArg1 = " (Parameter 'filename')";
    private const string ToFilenameArg2 = " (Parameter 'replaceInvalidCharsWith')";

    private static readonly TestCaseData[] sumCases = {
        new(10.Hours(), new[]{ 10.Hours() }),
        new(10.Hours(), new[]{ 5.Hours(), 5.Hours() }),
        new(10.Hours(), new[]{ 5, 3, 2 }.Select(s => s.Hours())),
        new(10.Hours(), new[]{ 0.3, 1.7, 4.6, 3.4 }.Select(s => s.Hours()))
    };

    private static readonly TestCaseData[] toFilenameCases = {
        new("  validFilename ", "_", "validFilename"),
        new("invalid/filename", "_", "invalid_filename"),
        new("  /\\invalid:?filename\0    ", "_", "__invalid__filename_"),
        new(@"  C:\This\Is\A\Path   ", "_", "C__This_Is_A_Path"),
        new("  validFilename    ", "!!", "validFilename"),
        new(" invalid/filename  ", "!!", "invalid!!filename"),
        new(" /\\invalid:?filename\0  ", "!!", "!!!!invalid!!!!filename!!"),
        new(@"  C:\This\Is\A\Path ", "!!", "C!!!!This!!Is!!A!!Path")
    };

    private static readonly TestCaseData[] toFilenameInvalidCases = {
        new(null, "", "Is null, empty, or whitespace."+ToFilenameArg1),
        new("", "", "Is null, empty, or whitespace."+ToFilenameArg1),
        new("  ", "", "Is null, empty, or whitespace."+ToFilenameArg1),
        new("f", null, "Is null or empty."+ToFilenameArg2),
        new("f", "", "Is null or empty."+ToFilenameArg2),
        new("f", "*", "Contains invalid filename chars."+ToFilenameArg2),
        new("f", "*/:?", "Contains invalid filename chars."+ToFilenameArg2),
        new("f", ".", "Consists only of dots."+ToFilenameArg2),
        new("f", "....", "Consists only of dots."+ToFilenameArg2)
    };

    [TestCaseSource(typeof(EqualContentCases))]
    public void TestEqualsContent<TKey, TValue>(IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2, bool areEquals)
        => Assert.That(d1.EqualsContent(d2), Is.EqualTo(areEquals));

    [TestCase("Test", "value", "<Tests><Test>value</Test></Tests>")]
    public void TestGetSingleNode(string name, string innerText, string xml)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);

        Assert.That(doc.GetSingleChild(name), Is.EqualTo(innerText));
    }

    [TestCase("<Tests/>", "'Tests' has no child named 'Test'.")]
    [TestCase("<Tests><Test/><Test/></Tests>", "'Tests' has 2 childs named 'Test' but only one was expected.")]
    public void TestGetSingleNodeException(string xml, string exceptionMessage)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);

        var e = Assert.Throws<XmlException>(() => doc.GetSingleChild("Test"));
        Assert.That(e!.Message, Is.EqualTo(exceptionMessage));
    }

    [TestCase("<Test>value</Test>", "", "value")]
    [TestCase("<Test xml:lang=\"fr\">valueFr</Test>", "fr", "valueFr")]
    public void TestSetFromXml(string xml, string cultureName, string value)
    {
        XmlDocument doc = new();
        doc.LoadXml(xml);
        LocalizedString str = new();

        str.SetFromXml(doc.DocumentElement.AssertNotNull());

        Assert.That(str.Single(), Is.EqualTo(KeyValuePair.Create(cultureName, value)));
    }

    [TestCaseSource(nameof(sumCases))]
    public void TestSum(TimeSpan total, IEnumerable<TimeSpan> elements)
        => Assert.That(total, Is.EqualTo(elements.Sum<TimeSpan>(ts => ts)));

    [TestCaseSource(nameof(toFilenameCases))]
    public void TestToFilename(string filename, string replaceInvalidCharsWith, string expectedResult) => Assert.That(filename.ToFilename(replaceInvalidCharsWith), Is.EqualTo(expectedResult));

    [TestCaseSource(nameof(toFilenameInvalidCases))]
    public void TestToFilenameInvalid(string filename, string replaceInvalidCharsWith, string exceptionMessage)
    {
        var e = Assert.Throws<ArgumentException>(() => filename.ToFilename(replaceInvalidCharsWith));
        Assert.That(e!.Message, Is.EqualTo(exceptionMessage));
    }

    private sealed class EqualContentCases : IEnumerable<TestCaseData>
    {
        private static Dictionary<int, string> Abc => new()
        {
            [1] = "a",
            [2] = "b",
            [3] = "c"
        };

        private static Dictionary<int, string> Acb => new()
        {
            [1] = "a",
            [2] = "c",
            [3] = "b"
        };

        private static Dictionary<int, object> Obj => new()
        {
            [1] = new(),
            [2] = new(),
            [3] = new()
        };

        public IEnumerator<TestCaseData> GetEnumerator()
        {
            yield return new(Empty<object, object>(), Empty<object, object>(), true);
            yield return new(Abc, Abc, true);
            yield return new(Obj, Obj, false);
            yield return new(Abc, Empty<int, string>(), false);
            yield return new(Abc, Acb, false);
            yield return new(Obj, Empty<int, object>(), false);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static Dictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull => new();
    }
}