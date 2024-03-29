﻿using Scover.WinClean.Services;

using static System.Environment;

namespace Scover.WinClean.Model;

/// <summary>Represents a directory containing files related to the application.</summary>
public static class AppDirectory
{
    /// <summary>Gets the full path to the application's logging directory.</summary>
    public static string Logs { get; } = Create(Path.GetTempPath(), ServiceProvider.Get<IApplicationInfo>().Name, nameof(Logs));

    /// <summary>Gets the full path to the application's scripts directory.</summary>
    public static string Scripts { get; } = Create(GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.DoNotVerify), ServiceProvider.Get<IApplicationInfo>().Name, nameof(Scripts));

    private static string Create(params string[] paths) => Directory.CreateDirectory(Path.Join(paths)).FullName;
}