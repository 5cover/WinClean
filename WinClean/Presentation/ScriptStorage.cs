using System.Collections;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation;

/// <summary>Represent the storage systems of mutable and immutable scripts.</summary>
public sealed class ScriptStorage : IEnumerable<Script>
{
    private const string DefaultScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(WinClean)}.{nameof(Scripts)}";
    private const string MetadataContentFilesNamespace = $"{nameof(Scover)}.{nameof(WinClean)}";

    private readonly List<ScriptRepository> _repos = new();
    private bool _loaded;

    private readonly IScriptSerializer _serializer;

    public ScriptStorage()
    {
        IScriptMetadataDeserializer d = new ScriptMetadataXmlDeserializer();
        // Explicitly enumerate the metadata lists.
        Metadatas = new()
        {
            d.GetCategories(ReadContentFile("Categories.xml")).ToList(),
            d.GetHosts(ReadContentFile("Hosts.xml")).ToList(),
            d.GetImpacts(ReadContentFile("Impacts.xml")).ToList(),
            d.GetRecommendationLevels(ReadContentFile("RecommendationLevels.xml")).ToList()
        };

        static Stream ReadContentFile(string filename)
            => AppInfo.Assembly.GetManifestResourceStream($"{MetadataContentFilesNamespace}.{filename}").AssertNotNull();

        _serializer = new ScriptXmlSerializer(Metadatas);
    }

    public TypedEnumerablesDictionary Metadatas { get; }

    private IEnumerable<MutableScriptRepository> MutableRepos => _repos.OfType<MutableScriptRepository>();
    private IEnumerable<Script> Scripts => _repos.SelectMany(repo => repo);

    private record StoredScript(Uri Source, Script script, bool IsMutable, bool IsOpenable);

    /// <summary>
    /// Deserializes a script from <paramref name="source"/>, adds it to the appropriate repository and returns it.
    /// </summary>
    /// <param name="type">The type of script to create.</param>
    /// <param name="source">The source of the script to add. It must exist.</param>
    /// <returns>The new script.</returns>
    /// <inheritdoc cref="MutableScriptRepository.Add(string)" path="/exception"/>
    public Script Add(ScriptType type, string source) => MutableRepos.Single(repo => repo.Type == type).Add(source);

    public IEnumerator<Script> GetEnumerator() => Scripts.GetEnumerator();

    public void Load(InvalidScriptDataCallback invalidScriptData, FSErrorCallback fsErrorReloadElseIgnore)
    {
        if (_loaded)
        {
            return;
        }
        _repos.Add(new EmbeddedScriptRepository(ScriptType.Default, DefaultScriptsResourceNamespace, _serializer));
        _repos.Add(new FileScriptRepository(ScriptType.Custom, AppDirectory.Scripts, App.Settings.ScriptFileExtension, _serializer, invalidScriptData, fsErrorReloadElseIgnore));

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

    IEnumerator IEnumerable.GetEnumerator() => Scripts.GetEnumerator();
}