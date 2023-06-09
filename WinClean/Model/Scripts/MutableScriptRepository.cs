using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public abstract class MutableScriptRepository : ScriptRepository
{
    protected MutableScriptRepository(IScriptSerializer serializer, ScriptType type) : base(serializer, type)
    { }

    /// <summary>Adds a new script with the specified source.</summary>
    /// <param name="source">The source of the script to add. It must exist.</param>
    /// <returns>The script that was added.</returns>
    /// <exception cref="FileSystemException">A filesystem exception occured.</exception>
    /// <exception cref="ScriptAlreadyExistsException">
    /// A script with <paramref name="source"/> as a source already exists in the repository.
    /// </exception>
    /// <exception cref="DeserializationException">Script deserialization failed.</exception>
    public abstract Script Add(string source);

    /// <summary>Adds a new script.</summary>
    /// <param name="script">The script to add.</param>
    /// <remarks>The source will be added to the repository only once <see cref="Save"/> is called.</remarks>
    /// <exception cref="ScriptAlreadyExistsException">
    /// <paramref name="script"/> already exists in the repository.
    /// </exception>
    public abstract void Add(Script script);

    /// <summary>Returns whether a source exists within a repository.</summary>
    /// <returns>
    /// <see langword="true"/> if <paramref name="source"/> exists in this repository, else <see
    /// langword="false"/>.
    /// </returns>
    public abstract bool Contains(string source);

    public abstract bool Contains(Script script);

    /// <summary>Removes a script from a repository at a specified source.</summary>
    /// <param name="source">The source of the script to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the script was successfully removed otherwise, <see langword="false"/>.
    /// This method also returns <see langword="false"/> if script was not found in the repository.
    /// </returns>
    /// <remarks>
    /// The removal will be performed immediately, and not when <see cref="Save"/> is called.
    /// </remarks>
    public abstract bool Remove(string source);

    /// <summary>Removes a script from a repository.</summary>
    /// <param name="source">The script to remove.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="script"/> successfully removed otherwise, <see
    /// langword="false"/>. This method also returns <see langword="false"/> if script was not found in the
    /// repository.
    /// </returns>
    /// <remarks>
    /// The removal will be performed immediately, and not when <see cref="Save"/> is called.
    /// </remarks>
    public abstract bool Remove(Script script);

    /// <summary>Saves the scripts to persistent storage.</summary>
    public abstract void Save();
}