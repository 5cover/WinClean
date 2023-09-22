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

    /// <inheritdoc cref="ScriptRepository.RetrieveScript(string)"/>
    Script RetrieveScript(ScriptType type, string source);

    Task LoadAsync(ScriptDeserializationErrorCallback scriptLoadError, FSErrorCallback fsErrorReloadElseIgnore);
}