using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model;

/// <summary>Callback called when a filesystem error occurs.</summary>
/// <param name="e">The filesystem exception that occurred.</param>
public delegate bool FSErrorCallback(FileSystemException e);

/// <summary>
/// Callback called when a script could not be created because it has invalid or missing data.
/// </summary>
/// <param name="exception">The exception that caused the error.</param>
/// <param name="source">The source of the script.</param>
/// <returns>A <see cref="InvalidScriptDataAction"/> value.</returns>
public delegate InvalidScriptDataAction ScriptDeserializationErrorCallback(DeserializationException exception, string source);

/// <summary>Callback called when an unhandled exception occurs.</summary>
/// <param name="exception">The exception that occurred</param>
/// <returns>
/// <see langword="true"/> if the application should continue, <see langword="false"/> otherwise.
/// </returns>
public delegate bool UnhandledExceptionCallback(Exception exception);

public enum InvalidScriptDataAction
{
    /// <summary>Attempt to reload the script</summary>
    Reload,

    /// <summary>Ignore the script.</summary>
    Ignore,

    /// <summary>Delete the script.</summary>
    Delete,
}