namespace Scover.WinClean.Model;

/// <summary>Information for executing a script host program that requires a temporary file.</summary>
public sealed class HostTempFileStartInfo : HostStartInfo, IDisposable
{
    private readonly string _incompleteArguments, _tempFileContents, _tempFileExtension;
    private readonly Lazy<TempFile> _tempFile;

    public HostTempFileStartInfo(string filename, string incompleteArguments, string tempFileContents, string tempFileExtension) : base(filename, "")
    {
        (_incompleteArguments, _tempFileContents, _tempFileExtension) = (incompleteArguments, tempFileContents, tempFileExtension);
        _tempFile = new(() => TempFile.Create("Script_{0}", _tempFileExtension, _tempFileContents));
    }

    public override string Arguments => _incompleteArguments.FormatWith(_tempFile.Value.Filename);

    public override void Dispose()
    {
        _tempFile.DisposeIfCreated();
        base.Dispose();
    }
}