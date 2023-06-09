using Semver;

namespace Scover.WinClean.Services;

/// <summary>Manages application settings.</summary>
public interface ISettings
{
    #region Readonly

    public SemVersionRange DefaultHostVersions { get; }
    public SemVersionRange DefaultScriptVersions { get; }
    string ScriptFileExtension { get; }

    #endregion Readonly

    double Height { get; set; }
    bool IsLoggingEnabled { get; set; }
    bool IsMaximized { get; set; }
    double Left { get; set; }

    TimeSpan ScriptDetectionTimeout { get; set; }

    /// <summary>
    /// Gets the script execution times dictionary, keyed by <see cref="IScript.InvariantName"/>.
    /// </summary>
    public IDictionary<string, TimeSpan> ScriptExecutionTimes { get; }

    bool ShowUpdateDialog { get; set; }
    double Top { get; set; }
    double Width { get; set; }

    /// <summary>Resets the settings to their default values.</summary>
    void Reset();

    /// <summary>Saves the settings to persistent storage.</summary>
    void Save();
}