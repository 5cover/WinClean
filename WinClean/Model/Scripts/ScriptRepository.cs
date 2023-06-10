using System.Collections;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public abstract class ScriptRepository : IReadOnlyCollection<Script>
{
    private bool _loaded;

    protected ScriptRepository(IScriptSerializer serializer, ScriptType type) => (Serializer, Type) = (serializer, type);

    public abstract int Count { get; }
    public ScriptType Type { get; }
    protected IScriptSerializer Serializer { get; }

    public abstract IEnumerator<Script> GetEnumerator();

    /// <summary>Loads the script from the persistent storage.</summary>
    /// <remarks>This collection will be empty before this method is called.</remarks>
    public void Load()
    {
        if (!_loaded)
        {
            LoadScripts();
            _loaded = true;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected abstract void LoadScripts();
}