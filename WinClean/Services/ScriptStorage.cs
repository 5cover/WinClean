using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Model.Serialization.Xml;

namespace Scover.WinClean.Services;

/// <summary>Represent the storage systems of mutable and immutable scripts.</summary>
public sealed class ScriptStorage : IScriptStorage
{
    private const string DefaultScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(WinClean)}.Scripts";
    private readonly Dictionary<ScriptType, ScriptRepository> _repos = new();
    private bool _loaded;
    public int ScriptCount => _repos.Values.Sum(r => r.Count);
    public IEnumerable<Script> Scripts => _repos.Values.SelectMany(repo => repo);
    public IScriptSerializer Serializer => new ScriptXmlSerializer();
    private sealed record StoredScript(Uri Source, Script script, bool IsMutable, bool IsOpenable);

    public Script Add(ScriptType type, string source) => GetMutableRepository(type).Add(source);

    public void Load(ScriptDeserializationErrorCallback scriptLoadError, FSErrorCallback fsErrorReloadElseIgnore)
    {
        if (_loaded)
        {
            return;
        }
        AddRepo(new EmbeddedScriptRepository(DefaultScriptsResourceNamespace, Serializer, ScriptType.Default));
        AddRepo(new FileScriptRepository(AppDirectory.Scripts, ServiceProvider.Get<ISettings>().ScriptFileExtension, scriptLoadError, fsErrorReloadElseIgnore, Serializer, ScriptType.Custom));

        foreach (var repo in _repos.Values)
        {
            repo.Reload();
        }
        _loaded = true;
    }

    public bool Remove(Script script) => GetMutableRepository(script.Type).Remove(script);

    public void Update(Script script) => GetMutableRepository(script.Type).Update(script);

    private void AddRepo(ScriptRepository repo) => _repos.Add(repo.Type, repo);

    private MutableScriptRepository GetMutableRepository(ScriptType type)
        => !_repos.TryGetValue(type, out var repo)
            ? throw new ArgumentException($"No repository found for script type '{type.InvariantName}'")
            : repo as MutableScriptRepository
            ?? throw new ArgumentException($"Repository for script type '{type.InvariantName}' is not mutable");
}