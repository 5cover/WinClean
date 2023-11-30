namespace Scover.WinClean.Model;

/// <summary>Information for executing a script host program that requires a temporary file.</summary>
public sealed class HostTempFileStartInfo : HostStartInfo
{
    private readonly string _incompleteArguments;
    private readonly Lazy<TempFile> _tempFile;

    public HostTempFileStartInfo(string filename, string incompleteArguments, string tempFileContents, string tempFileExtension) : base(filename, "")
    {
        _incompleteArguments = incompleteArguments;
        _tempFile = new(() => TempFile.Create("Script_{0}", tempFileExtension, tempFileContents));
    }

    public override string Arguments => _incompleteArguments.FormatWith(_tempFile.Value.Filename);

    public override void Dispose()
    {
        _tempFile.DisposeIfCreated();
        base.Dispose();
    }
}