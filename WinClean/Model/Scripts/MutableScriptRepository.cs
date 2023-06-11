using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public abstract class MutableScriptRepository : ScriptRepository
{
    protected MutableScriptRepository(IScriptSerializer serializer, ScriptType type) : base(serializer, type)
    { }

    /// <summary>Adds a script with the specified source.</summary>
    /// <param name="source">The source of the script to add. It must not exist in the repository.</param>
    /// <returns>The script that was added.</returns>
    /// <exception cref="FileSystemException">A filesystem exception occured.</exception>
    /// <exception cref="DeserializationException">Script deserialization failed.</exception>
    /// <exception cref="ScriptAlreadyExistsException">
    /// <paramref name="script"/> already exists in the repository.
    /// </exception>
    public abstract Script Add(string source);

    /// <summary>Returns whether a source exists within a repository.</summary>
    /// <returns>
    /// <see langword="true"/> if <paramref name="source"/> exists in this repository, else <see
    /// langword="false"/>.
    /// </returns>
    public abstract bool Contains(string source);

    /// <summary>Returns whether a script exists within a repository.</summary>
    /// <returns>
    /// <see langword="true"/> if <paramref name="script"/> exists in this repository, else <see
    /// langword="false"/>.
    /// </returns>
    public abstract bool Contains(Script script);

    /// <summary>Removes a script from a repository at a specified source.</summary>
    /// <param name="source">The source of the script to remove.</param>
    /// <returns>
    /// <see langword="true"/> if the script was successfully removed otherwise, <see langword="false"/>.
    /// This method also returns <see langword="false"/> if script was not found in the repository.
    /// </returns>
    public abstract bool Remove(string source);

    /// <summary>Removes a script from a repository.</summary>
    /// <param name="source">The script to remove.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="script"/> successfully removed otherwise, <see
    /// langword="false"/>. This method also returns <see langword="false"/> if script was not found in the
    /// repository.
    /// </returns>
    public abstract bool Remove(Script script);
}