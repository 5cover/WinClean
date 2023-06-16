using static System.IO.Path;

namespace Scover.WinClean.Model;

/// <summary>Encapsulates a temporary file.</summary>
public sealed class TempFile : IDisposable
{
    private TempFile(string name, string extension) => Filename = Join(GetTempPath(), ChangeExtension(name.FormatWith(GetRandomFileName()), extension));

    public string Filename { get; }

    /// <summary>Creates a temporary file.</summary>
    /// <param name="name">
    /// The name of the temporary file, a formattable string with one argument, the random component of the
    /// name.
    /// </param>
    /// <param name="extension">The extension of the temporary file.</param>
    /// <param name="contents">The contents of the temporary file.</param>
    /// <returns>
    /// A new <see cref="TempFile"/> instances representing the newly created temporary file.
    /// </returns>
    public static TempFile Create(string name, string extension, string contents)
    {
        TempFile tempFile = new(name, extension);
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