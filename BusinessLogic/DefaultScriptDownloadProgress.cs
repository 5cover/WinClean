namespace Scover.WinClean.BusinessLogic;

/// <summary>The progress of a script download operation.</summary>
public readonly struct DefaultScriptDownloadProgress
{
    /// <summary>Gets or inits the index of the default script that is currently being downloaded.</summary>
    public int CurrentScriptIndex { get; init; }

    public string CurrentScriptName { get; init; }

    /// <summary>Gets or inits the total count of default scripts to download.</summary>
    public int ScriptCount { get; init; }

    /// <summary>Gets or inits the name of the script that is currently being downloaded.</summary>
}