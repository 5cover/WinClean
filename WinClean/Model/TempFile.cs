using static System.IO.Path;

namespace Scover.WinClean.Model;

/// <summary>Encapsulates a temporary file.</summary>
public sealed class TempFile : IDisposable
{
    private TempFile(string extension) => Filename = Join(GetTempPath(), ChangeExtension(GetRandomFileName(), extension));

    public string Filename { get; }

    public static TempFile Create(string extension, string contents)
    {
        TempFile tempFile = new(extension);
        File.WriteAllText(tempFile.Filename, contents);
        return tempFile;
    }

    public void Delete()
    {
        try
        {
            File.Delete(Filename);
        }
        catch (Exception ex) when (ex.IsFileSystemExogenous())
        {
            // It's a temp file, it's fine not to delete it.
        }
    }

    public void Dispose() => Delete();
}