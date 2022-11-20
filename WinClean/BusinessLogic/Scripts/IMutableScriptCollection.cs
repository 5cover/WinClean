namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A mutable collection of scripts.</summary>
public interface IMutableScriptCollection : IEnumerable<Script>
{
    /// <summary>Adds a new script to the storage system.</summary>
    /// <param name="script">The script to add.</param>
    /// <exception cref="ScriptAlreadyExistsException">An identical script already exists in the storage system.</exception>
    public void Add(Script script);

    /// <summary>Removes a script from the storage system..</summary>
    /// <param name="script">The script to remove.</param>
    void Remove(Script script);

    /// <summary>Commits all changes to the scripts to the storage system.</summary>
    void Save();
}