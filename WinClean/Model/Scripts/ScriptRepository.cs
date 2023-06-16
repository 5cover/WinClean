using System.Collections;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public abstract class ScriptRepository : IReadOnlyCollection<Script>
{
    protected ScriptRepository(IScriptSerializer serializer, ScriptType type) => (Serializer, Type) = (serializer, type);

    public abstract int Count { get; }
    public ScriptType Type { get; }
    protected IScriptSerializer Serializer { get; }

    public abstract IEnumerator<Script> GetEnumerator();

    /// <summary>Reloads the script from the persistent storage.</summary>
    public void Reload()
    {
        Clear();
        Load();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected abstract void Clear();

    protected abstract void Load();
}