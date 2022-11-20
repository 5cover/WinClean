using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Callback called when a script is hung.</summary>
/// <param name="script">The hung script.</param>
public delegate bool HungScriptCallback(Script script);

/// <summary>Callback called a script could not be created because it has invalid or missing data.</summary>
/// <param name="exception">The exception that caused the error.</param>
/// <param name="source">The source of the script.</param>
public delegate bool InvalidScriptDataCallback(Exception exception, string source);

/// <summary>Callback called when a filesystem error occurs.</summary>
/// <param name="e">The filesystem exception that occured.</param>
/// <param name="verb">The filesystem action that failed.</param>
/// <param name="fsInfo">The filesystem element that was being operated on.</param>
public delegate bool FSErrorCallback(Exception e, FSVerb verb, FileSystemInfo fsInfo);