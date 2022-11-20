using Scover.WinClean.DataAccess;

namespace Tests;

[TestOf(typeof(ExtensionGroup))]
[TestFixture("Text Document", new[] { ".txt" })]
[TestFixture("Text Document", new[] { ".txt", ".exe" })]
public sealed class ExtensionGroupTests
{
    private readonly ExtensionGroup _extensionGroup;
    private readonly string _name;

    public ExtensionGroupTests(string name, string[] extensions) => (_name, _extensionGroup) = (name, new ExtensionGroup(extensions));

    [Test]
    public void TestName() => Assert.That(_extensionGroup.Name, Is.EqualTo(_name));
}