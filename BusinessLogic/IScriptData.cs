using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Common properties for scripts and their metadata.</summary>
public interface IScriptData
{
    /// <summary>Gets the description.</summary>
    /// <value>A free-form description.</value>
    string Description { get; }

    /// <summary>Gets the invariant name.</summary>
    /// <value>A culture-independent, human-readable name for this instance that can be used to identify the value programatically.</value>
    string InvariantName { get; }

    /// <summary>Gets the name.</summary>
    /// <value>An user friendly name.</value>
    string Name { get; }
}