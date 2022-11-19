using System.Collections;
using System.Xml;

using Humanizer;

using Scover.WinClean.DataAccess;

namespace Tests;

[TestFixture(TestOf = typeof(Helpers))]
public sealed class HelpersTests
{
    private const string absolute = "C:/aBSOLutE";
    private const string relativeDir = "rElaTIve/CURreNTdir";
    private const string relativeDrive = "/reLaTivE/CURRENTdrivERooT";

    private static readonly TestCaseData[] isPathInDirectoryCases = new TestCaseData[]
    {
        new(absolute, absolute, false),
        new($"{absolute}/..", absolute, false),
        new($"{absolute}/Inside", absolute, true),
        new($"{absolute}/Inside/..", absolute, false),
        new($"{absolute}/Inside/../Inside", absolute, true),
        new(relativeDir, relativeDir, false),
        new($"{relativeDir}/..", relativeDir, false),
        new($"{relativeDir}/Inside", relativeDir, true),
        new($"{relativeDir}/Inside/..", relativeDir, false),
        new($"{relativeDir}/Inside/../Inside", relativeDir, true),
        new(relativeDrive, relativeDrive, false),
        new($"{relativeDrive}/..", relativeDrive, false),
        new($"{relativeDrive}/Inside", relativeDrive, true),
        new($"{relativeDrive}/Inside/..", relativeDrive, false),
        new($"{relativeDrive}/Inside/../Inside", relativeDrive, true),
    };

    private static readonly TestCaseData[] sumCases = new TestCaseData[]
    {
        new(10.Hours(), new[]{ 10.Hours() }),
        new(10.Hours(), new[]{ 5.Hours(), 5.Hours() }),
        new(10.Hours(), new[]{ 5, 3, 2 }.Select(s => s.Hours())),
        new(10.Hours(), new[]{ 0.3, 1.7, 4.6, 3.4 }.Select(s => s.Hours())),
    };

    private static readonly TestCaseData[] toFilenameCases = new TestCaseData[]
    {
        new("  validFilename ", "_", "validFilename"),
        new("invalid/filename", "_", "invalid_filename"),
        new("  /\\invalid:?filename\0    ", "_", "__invalid__filename_"),
        new(@"  C:\This\Is\A\Path   ", "_", "C__This_Is_A_Path"),
        new("  validFilename    ", "!!", "validFilename"),
        new(" invalid/filename  ", "!!", "invalid!!filename"),
        new(" /\\invalid:?filename\0  ", "!!", "!!!!invalid!!!!filename!!"),
        new(@"  C:\This\Is\A\Path ", "!!", "C!!!!This!!Is!!A!!Path"),
    };

    private static readonly TestCaseData[] toFilenameInvalidCases = new TestCaseData[]
    {
        new(null, "", "Is null, empty, or whitespace. (Parameter 'filename')"),
        new("", "", "Is null, empty, or whitespace. (Parameter 'filename')"),
        new("  ", "", "Is null, empty, or whitespace. (Parameter 'filename')"),
        new("f", null, "Is null or empty. (Parameter 'replaceInvalidCharsWith')"),
        new("f", "", "Is null or empty. (Parameter 'replaceInvalidCharsWith')"),
        new("f", "*", "Contains invalid filename chars. (Parameter 'replaceInvalidCharsWith')"),
        new("f", "*/:?", "Contains invalid filename chars. (Parameter 'replaceInvalidCharsWith')"),
        new("f", ".", "Consists only of dots. (Parameter 'replaceInvalidCharsWith')"),
        new("f", "....", "Consists only of dots. (Parameter 'replaceInvalidCharsWith')"),
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
        Assert.That(e.Message, Is.EqualTo(exceptionMessage));
    }

    [TestCaseSource(nameof(isPathInDirectoryCases))]
    public void TestIsPathInDirectory(string path, string directory, bool result) => Assert.That(path.IsPathInDirectory(directory), Is.EqualTo(result));

    [TestCaseSource(nameof(isPathInDirectoryCases))]
    public void TestIsPathInDirectoryCaseSensitive(string path, string directory, bool result) => Assert.That(path.IsPathInDirectory(directory, false), Is.EqualTo(result));

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
        => Assert.That(total, Is.EqualTo(elements.Sum(ts => ts)));

    [TestCaseSource(nameof(toFilenameCases))]
    public void TestToFilename(string filename, string replaceInvalidCharsWith, string expectedResult) => Assert.That(filename.ToFilename(replaceInvalidCharsWith), Is.EqualTo(expectedResult));

    [TestCaseSource(nameof(toFilenameInvalidCases))]
    public void TestToFilenameInvalid(string filename, string replaceInvalidCharsWith, string exceptionMessage)
    {
        var e = Assert.Throws<ArgumentException>(() => filename.ToFilename(replaceInvalidCharsWith));
        Assert.That(e.Message, Is.EqualTo(exceptionMessage));
    }

    private sealed class EqualContentCases : IEnumerable<TestCaseData>
    {
        private static readonly Dictionary<int, string> abc = new()
        {
            [1] = "a",
            [2] = "b",
            [3] = "c"
        };

        private static readonly Dictionary<int, string> acb = new()
        {
            [1] = "a",
            [2] = "c",
            [3] = "b"
        };

        private static Dictionary<int, object> obj => new()
        {
            [1] = new object(),
            [2] = new object(),
            [3] = new object()
        };

        public IEnumerator<TestCaseData> GetEnumerator()
        {
            yield return new(Empty<object, object>(), Empty<object, object>(), true);
            yield return new(abc, abc, true);
            yield return new(obj, obj, false);
            yield return new(abc, Empty<int, string>(), false);
            yield return new(abc, acb, false);
            yield return new(obj, Empty<int, object>(), false);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static Dictionary<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull => new();
    }
}