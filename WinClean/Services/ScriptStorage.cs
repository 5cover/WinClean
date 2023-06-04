using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Model.Serialization.Xml;
namespace Scover.WinClean.Services;

/// <summary>Represent the storage systems of mutable and immutable scripts.</summary>
public sealed class ScriptStorage : IScriptStorage
{
    private const string DefaultScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(WinClean)}.{nameof(Scripts)}";
    private readonly List<ScriptRepository> _repos = new();
    private bool _loaded;
    public int ScriptCount => _repos.Sum(r => r.Count);
    public IEnumerable<Script> Scripts => _repos.SelectMany(repo => repo);
    public IScriptSerializer Serializer => new ScriptXmlSerializer(ServiceProvider.Get<ISettings>().DefaultScriptSupportedVersionRange);
    private IEnumerable<MutableScriptRepository> MutableRepos => _repos.OfType<MutableScriptRepository>();
    private sealed record StoredScript(Uri Source, Script script, bool IsMutable, bool IsOpenable);

    /// <summary>
    /// Deserializes a script from <paramref name="source"/>, adds it to the appropriate repository and
    /// returns it.
    /// </summary>
    /// <param name="type">The type of script to create.</param>
    /// <param name="source">The source of the script to add. It must exist.</param>
    /// <returns>The new script.</returns>
    /// <inheritdoc cref="MutableScriptRepository.Add(string)" path="/exception"/>
    public Script Add(ScriptType type, string source) => MutableRepos.Single(repo => repo.Type == type).Add(source);

    public void Load(InvalidScriptDataCallback invalidScriptData, FSErrorCallback fsErrorReloadElseIgnore)
    {
        if (_loaded)
        {
            return;
        }
        _repos.Add(new EmbeddedScriptRepository(DefaultScriptsResourceNamespace, Serializer, ScriptType.Default));
        _repos.Add(new FileScriptRepository(AppDirectory.Scripts, ServiceProvider.Get<ISettings>().ScriptFileExtension, invalidScriptData, fsErrorReloadElseIgnore, Serializer, ScriptType.Custom));

        foreach (var repo in _repos)
        {
            repo.Load();
        }
        _loaded = true;
    }

    public bool Remove(Script script) => MutableRepos.Single(repo => repo.Type == script.Type).Remove(script);

    public void Save()
    {
        foreach (var mutableRepo in MutableRepos)
        {
            mutableRepo.Save();
        }
    }

    public void Save(IEnumerable<Script> newScripts)
    {
        // Remove scripts first to avoid duplicates.
        foreach (var removedScript in Scripts.Except(newScripts))
        {
            _ = Remove(removedScript);
        }
        foreach (var addedScript in newScripts.Except(Scripts))
        {
            MutableRepos.Single(repo => repo.Type == addedScript.Type).Add(addedScript);
        }
        Save();
    }
}