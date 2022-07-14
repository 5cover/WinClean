using static System.Environment;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Represents a directory containing files related to the application.</summary>
public sealed class AppDirectory
{
    private readonly DirectoryInfo _info;

    private AppDirectory(string path) => _info = new(path);

    private AppDirectory(string path, string name) : this(Path.Join(path, AppInfo.Name, name))
    {
    }

    private AppDirectory(SpecialFolder location, string name) : this(Path.Join(GetFolderPath(location, SpecialFolderOption.Create)), name)
    {
    }

    /// <summary>The application's install directory.</summary>
    public static AppDirectory InstallDir { get; } = new(AppContext.BaseDirectory);

    /// <summary>The application's logging directory.</summary>
    public static AppDirectory LogDir { get; } = new(Path.GetTempPath(), "Logs");

    /// <summary>The directory where scripts are stored.</summary>
    public static AppDirectory ScriptsDir { get; } = new(SpecialFolder.CommonApplicationData, "Scripts");

    /// <summary>Returns the <see cref="DirectoryInfo"/> object representing this directory.</summary>
    /// <remarks>Creates the directory if it doesn't exist.</remarks>
    public DirectoryInfo Info
    {
        get
        {
            _info.Create();
            return _info;
        }
    }

    /// <summary>Joins the path of the directory with the specified path components.</summary>
    /// <param name="paths">The path components to join with the directory.</param>
    /// <returns>The concatenated path.</returns>
    public string Join(params string[] paths) => Path.Join(paths.Prepend(Info.FullName).ToArray());
}