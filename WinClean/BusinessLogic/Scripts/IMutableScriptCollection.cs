namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A mutable collection of scripts.</summary>
public interface IMutableScriptCollection : IEnumerable<Script>
{
    /// <summary>Adds a new script to the storage system.</summary>
    /// <param name="script">The script to add.</param>
    /// <exception cref="ScriptAlreadyExistsException">An identical script already exists in the storage system.</exception>
    public void Add(Script script);

    /// <summary>Deserializes and adds a new script to the storage system from a distant source.</summary>
    /// <param name="source">A string that identifies this script outside of this storage system.</param>
    /// <exception cref="ArgumentException"><paramref name="source"/> points to this storage system.</exception>
    /// <exception cref="InvalidDataException">The script at <paramref name="source"/> is in an incomplete or invalid format.</exception>
    /// <exception cref="ScriptAlreadyExistsException">An identical script already exists in the storage system.</exception>
    /// <returns>The script that was added.</returns>
    public Script Add(string source);

    /// <summary>Removes a script from the storage system..</summary>
    /// <param name="script">The script to remove.</param>
    void Remove(Script script);

    /// <summary>Commits all changes to the scripts to the storage system.</summary>
    void Save();
}