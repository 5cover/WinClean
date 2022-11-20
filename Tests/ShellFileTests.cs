using Scover.WinClean.DataAccess;

namespace Tests;

// nstoskrnl.exe is used because its file description is not localized.
[TestFixture("%systemroot%\\System32\\ntoskrnl.exe", "NT Kernel & System", TestOf = typeof(ShellFile))]
[TestFixture("%systemroot%\\system.ini", "", TestOf = typeof(ShellFile))]
public sealed class ShellFileTests : IDisposable
{
    private readonly string _fileDescription;

    private readonly ShellFile _shellFile;

    public ShellFileTests(string path, string fileDescription)
    {
        _shellFile = new(Environment.ExpandEnvironmentVariables(path));
        _fileDescription = fileDescription;
    }

    public void Dispose() => _shellFile.Dispose();

    [Test]
    public void TestFileDescription() => Assert.That(_shellFile.FileDescription, Is.EqualTo(_fileDescription));
}