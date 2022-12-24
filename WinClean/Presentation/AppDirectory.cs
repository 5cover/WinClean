using Scover.WinClean.BusinessLogic;

namespace Scover.WinClean.Presentation;

/// <summary>Represents a directory containing files related to the application.</summary>
public static class AppDirectory
{
    /// <summary>Gets the full path to the application's logging directory.</summary>
    public static string Logs { get; } = Create(Path.GetTempPath(), AppMetadata.Name, nameof(Logs));

    /// <summary>Gets the full path to the application's scripts directory.</summary>
    public static string Scripts { get; } = Create(GetFolderPath(Environment.SpecialFolder.ApplicationData), AppMetadata.Name, nameof(Scripts));

    private static string Create(params string[] paths) => Directory.CreateDirectory(Path.Join(paths)).FullName;

    private static string GetFolderPath(Environment.SpecialFolder specialFolder) => Environment.GetFolderPath(specialFolder, Environment.SpecialFolderOption.DoNotVerify);
}