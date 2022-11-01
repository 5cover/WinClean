namespace Scover.WinClean.BusinessLogic;

/// <summary>Callback called a script could not be deserialized because it has invalid or missing data.</summary>
/// <param name="exception">The exception that caused the error.</param>
/// <param name="path">The path to the script file that has invalid or missing data.</param>
public delegate bool InvalidScriptDataCallback(Exception exception, string path);

/// <summary>Callback called when a script is hung.</summary>
/// <param name="scriptName">The name of the hung script.</param>
public delegate bool HungScriptCallback(string scriptName);