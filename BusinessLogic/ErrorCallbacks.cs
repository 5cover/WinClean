namespace Scover.WinClean.BusinessLogic;

/// <summary>Callback called when a filesystem operation fails.</summary>
/// <param name="exception">The filesystem exception that caused the error.</param>
/// <param name="verb">What was being done.</param>
/// <param name="fsInfo">The file or directory that was being manipulated.</param>
/// <returns>
/// <see langword="true"/> if the filesystem operation should be retried; <see langword="false"/> if it should fail and throw
/// <paramref name="exception"/>.
/// </returns>
public delegate bool FSOperationCallback(Exception exception, FSVerb verb, FileSystemInfo fsInfo);

/// <summary>Callback called a script could not be deserialized because it has invalid or missing data.</summary>
/// <param name="exception">The exception that caused the error.</param>
/// <param name="path">The path to the script file that has invalid or missing data.</param>
public delegate bool InvalidScriptDataCallback(Exception exception, string path);

/// <summary>Callback called when a script is hung.</summary>
/// <param name="scriptName">The name of the hung script.</param>
/// <returns>
/// <see langword="true"/> if the script should keep running; <see langword="false"/> if the script host process should be killed.
/// </returns>
public delegate bool HungScriptCallback(string scriptName);