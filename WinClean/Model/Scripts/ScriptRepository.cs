using System.Collections.ObjectModel;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public abstract class ScriptRepository
{
    private readonly ObservableCollection<Script> _items = new();

    protected ScriptRepository(IScriptSerializer serializer, ScriptType type)
    {
        Scripts = new(_items);
        (Serializer, Type) = (serializer, type);
    }

    public ReadOnlyObservableCollection<Script> Scripts { get; }
    public ScriptType Type { get; }
    protected IScriptSerializer Serializer { get; }

    public abstract Script GetScript(string source);

    public abstract Task LoadAsync();

    protected void AddItem(Script script) => _items.Add(script);

    protected bool RemoveItem(Script script) => _items.Remove(script);
}