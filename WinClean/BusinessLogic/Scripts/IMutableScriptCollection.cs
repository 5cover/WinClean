namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A mutable collection of scripts.</summary>
public interface IMutableScriptCollection : IEnumerable<Script>
{
    /// <summary>Adds a new script to the storage system.</summary>
    /// <param name="script">The script to add.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="script"/> is already in this collection.
    /// </exception>
    public void Add(Script script);

    /// <summary>Removes a script from the storage system..</summary>
    /// <param name="script">The script to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the script is successfully removed; otherwise, <see langword="false"/>.
    /// This method also returns <see langword="false"/> if script was not found.
    /// </returns>
    bool Remove(Script script);

    /// <summary>Commits all changes to the scripts to the storage system.</summary>
    void Save();
}