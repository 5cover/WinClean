using static System.Environment;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Represents a directory containing files related to the application.</summary>
public sealed class AppDirectory
{
    private AppDirectory(params string[] paths)
    {
        Info = new(Path.Join(paths));
        Info.Create();
    }

    /// <summary>The application's default scripts directory.</summary>
    public static AppDirectory DefaultScriptsDir { get; } = new(AppContext.BaseDirectory, "Scripts");

    /// <summary>The application's install directory.</summary>
    public static AppDirectory InstallDir { get; } = new(AppContext.BaseDirectory);

    /// <summary>The application's logging directory.</summary>
    public static AppDirectory LogDir { get; } = new(Path.GetTempPath(), AppInfo.Name, "Logs");

    /// <summary>The directory where scripts created by the user are stored.</summary>
    public static AppDirectory ScriptsDir { get; } = new(GetFolderPath(SpecialFolder.ApplicationData), AppInfo.Name, "Scripts");

    /// <summary>Returns the <see cref="DirectoryInfo"/> object representing this directory.</summary>
    /// <remarks>Creates the directory if it doesn't exist.</remarks>
    public DirectoryInfo Info { get; }

    /// <summary>Joins the path of the directory with the specified path components.</summary>
    /// <param name="paths">The path components to join with the directory.</param>
    /// <returns>The concatenated path.</returns>
    public string Join(params string[] paths) => Path.Join(paths.Prepend(Info.FullName).ToArray());

    private static string GetFolderPath(SpecialFolder specialFolder) => Environment.GetFolderPath(specialFolder, SpecialFolderOption.DoNotVerify);
}