using System.Collections.ObjectModel;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Services;

public interface IScriptStorage
{
    ObservableCollection<Script> Scripts { get; }
    IScriptSerializer Serializer { get; }

    /// <summary>Commits changes to a script in storage.</summary>
    /// <param name="script">The script to update.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="script"/> could not be fond in storage.
    /// </exception>
    /// <exception cref="FileSystemException">A filesystem error occured.</exception>
    void Commit(Script script);

    /// <summary>Deserializes a script from <paramref name="source"/>.</summary>
    /// <param name="type">The type of script to create.</param>
    /// <param name="source">The source of the script.</param>
    /// <returns>The new script.</returns>
    /// <exception cref="ArgumentException">
    /// The script at <paramref name="source"/> could not be found.
    /// </exception>
    /// <exception cref="DeserializationException">The script could not be deserialized.</exception>
    /// <exception cref="FileSystemException">A filesystem error occured.</exception>
    Script GetScript(ScriptType type, string source);

    Task LoadAsync(ScriptDeserializationErrorCallback scriptLoadError, FSErrorCallback fsErrorReloadElseIgnore);
}