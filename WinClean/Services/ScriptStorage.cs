using System.Collections.ObjectModel;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Model.Serialization.Xml;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Services;

/// <summary>Represent the storage systems of mutable and immutable scripts.</summary>
public sealed class ScriptStorage : IScriptStorage
{
    private const string DefaultScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(WinClean)}.Scripts";
    private readonly Dictionary<ScriptType, ScriptRepository> _repos = new();
    private bool _loaded;
    public ObservableCollection<Script> Scripts { get; } = new();
    public IScriptSerializer Serializer => new ScriptXmlSerializer();

    /// <inheritdoc cref="MutableScriptRepository.Commit(Script)"/>
    public void Commit(Script script) => GetMutableRepository(script.Type).Commit(script);

    public Script RetrieveScript(ScriptType type, string source) => _repos[type].RetrieveScript(source);

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
        repo.Scripts.SendUpdatesTo(Scripts);
        Scripts.SendUpdatesTo(repo.Scripts, filter: script => script.Type == repo.Type);
    }

    private MutableScriptRepository GetMutableRepository(ScriptType type)
    => !_repos.TryGetValue(type, out var repo)
        ? throw new ArgumentException(ExceptionMessages.RepoNotFound.FormatWith(type.InvariantName), nameof(type))
        : repo as MutableScriptRepository
        ?? throw new ArgumentException(ExceptionMessages.RepoNotMutable.FormatWith(type.InvariantName), nameof(type));
}