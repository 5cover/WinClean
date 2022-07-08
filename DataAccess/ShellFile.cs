using Microsoft.WindowsAPICodePack.PropertySystem;

namespace Scover.WinClean.DataAccess;

/// <summary>Wrapper for <see cref="Microsoft.WindowsAPICodePack.Shell.ShellFile"/>.</summary>
public class ShellFile : IDisposable
{
    private readonly Microsoft.WindowsAPICodePack.Shell.ShellFile shFile;

    public ShellFile(FileInfo source) => shFile = new(source.FullName);

    /// <summary>Gets the "File Description" property of the object</summary>
    public string FileDescription
    {
        get
        {
            const string descriptionPropertyGuid = "0CEF7D53-FA64-11D1-A203-0000F81FEDEE";
            const int descriptionPropertyIndex = 3;
            return shFile.Properties.GetProperty<string>(new PropertyKey(descriptionPropertyGuid, descriptionPropertyIndex)).Value;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected virtual void Dispose(bool disposing) => shFile.Dispose();
}