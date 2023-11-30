using System.Collections.ObjectModel;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;

namespace Scover.WinClean.Services;

public interface IScriptStorage
{
    ObservableCollection<Script> Scripts { get; }

    /// <summary>Commits changes to a script in storage.</summary>
    /// <param name="script">The script to update.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="script"/> could not be fond in storage.
    /// </exception>
    /// <exception cref="FileSystemException">A filesystem error occurred.</exception>
    void Commit(Script script);

    Task LoadAsync(ScriptDeserializationErrorCallback scriptLoadError, FSErrorCallback fsErrorReloadElseIgnore);

    /// <inheritdoc cref="ScriptRepository.RetrieveScript(string)"/>
    Script RetrieveScript(ScriptType type, string source);
}