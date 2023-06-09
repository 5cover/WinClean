using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Services;

public interface IScriptStorage
{
    int ScriptCount { get; }
    IEnumerable<Script> Scripts { get; }
    IScriptSerializer Serializer { get; }

    /// <summary>
    /// Deserializes a script from <paramref name="source"/>, adds it to the appropriate repository and
    /// returns it.
    /// </summary>
    /// <param name="type">The type of script to create.</param>
    /// <param name="source">The source of the script to add. It must exist.</param>
    /// <returns>The new script.</returns>
    /// <inheritdoc cref="MutableScriptRepository.Add(string)" path="/exception"/>
    Script Add(ScriptType type, string source);

    void Load(ScriptDeserializationErrorCallback scriptLoadError, FSErrorCallback fsErrorReloadElseIgnore);

    bool Remove(Script script);

    void Save();

    void Save(IEnumerable<Script> newScripts);
}