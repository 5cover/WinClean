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

    Script Add(ScriptType type, string source);
    void Load(InvalidScriptDataCallback invalidScriptData, FSErrorCallback fsErrorReloadElseIgnore);
    bool Remove(Script script);
    void Save();
    void Save(IEnumerable<Script> newScripts);
}
