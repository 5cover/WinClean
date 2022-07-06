namespace Scover.WinClean.Presentation;

/// <summary>Represents a subdirectory of the application's install directory.</summary>
public abstract class AppSubdirectory
{
    /// <summary>Returns the <see cref="DirectoryInfo"/> object representing this subdirectory.</summary>
    public virtual DirectoryInfo Info
    {
        get
        {
            DirectoryInfo info = new(Path.Join(App.InstallDirectory, Name));
            info.Create();
            return info;
        }
    }

    /// <summary>Gets the name of this subdirectory.</summary>
    public abstract string Name { get; }

    /// <summary>Joins the specified path components with the path of the subdirectory.</summary>
    /// <param name="paths">The path components to join with the subdirectory.</param>
    /// <returns>The concatenated path.</returns>
    public virtual string Join(params string[] paths) => Path.Join(paths.Prepend(Info.FullName).ToArray());
}