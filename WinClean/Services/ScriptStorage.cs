using System.Collections.ObjectModel;

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
    public ObservableCollection<Script> Scripts { get; } = new();
    public IScriptSerializer Serializer => new ScriptXmlSerializer();

    public void Commit(Script script) => GetMutableRepository(script.Type).Commit(script);

    public Script GetScript(ScriptType type, string source) => _repos[type].GetScript(source);

    public async Task LoadAsync(ScriptDeserializationErrorCallback scriptLoadError, FSErrorCallback fsErrorReloadElseIgnore)
    {
        if (_loaded)
        {
            return;
        }

        AddRepo(new EmbeddedScriptRepository(DefaultScriptsResourceNamespace, Serializer, ScriptType.Default));
        AddRepo(new FileScriptRepository(AppDirectory.Scripts, ServiceProvider.Get<ISettings>().ScriptFileExtension, scriptLoadError, fsErrorReloadElseIgnore, Serializer, ScriptType.Custom));

        foreach (var repo in _repos.Values)
        {
            await repo.LoadAsync();
        }
        _loaded = true;
    }

    private void AddRepo(ScriptRepository repo)
    {
        _repos.Add(repo.Type, repo);
        repo.Scripts.SendUpdatesTo(Scripts, filter: s => true);
        Scripts.SendUpdatesTo(repo.Scripts, filter: script => script.Type == repo.Type);
    }

    private MutableScriptRepository GetMutableRepository(ScriptType type)
    => !_repos.TryGetValue(type, out var repo)
        ? throw new ArgumentException($"No repository found for script type '{type.InvariantName}'")
        : repo as MutableScriptRepository
        ?? throw new ArgumentException($"Repository for script type '{type.InvariantName}' is not mutable");
}