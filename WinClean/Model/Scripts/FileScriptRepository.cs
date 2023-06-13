using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

public sealed class FileScriptRepository : MutableScriptRepository
{
    private readonly string _directory;
    private readonly FSErrorCallback _fsErrorReloadElseIgnore;
    private readonly string _scriptFileExtension;
    private readonly ScriptDeserializationErrorCallback _scriptLoadError;
    private readonly BidirectionalDictionary<string, Script> _scripts = new();

    public FileScriptRepository(string directory,
                                string scriptFileExtension,
                                ScriptDeserializationErrorCallback scriptLoadError,
                                FSErrorCallback fsErrorReloadElseIgnore,
                                IScriptSerializer serializer,
                                ScriptType type) : base(serializer, type)
        => (_directory, _scriptFileExtension, _scriptLoadError, _fsErrorReloadElseIgnore)
            = (directory, scriptFileExtension, scriptLoadError, fsErrorReloadElseIgnore);

    public override int Count => _scripts.Count;

    public override Script Add(string source)
    {
        try
        {
            using Stream file = File.OpenRead(source);

            var newScript = Serializer.Deserialize(Type, file);
            Add(Path.GetFileName(source), newScript);
            return newScript;
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, FSVerb.Access, source);
        }
    }

    public override bool Contains(Script script) => _scripts.ContainsValue(script);

    public override bool Contains(string source) => _scripts.ContainsKey(source);

    public override IEnumerator<Script> GetEnumerator() => _scripts.Values.GetEnumerator();

    public override bool Remove(Script script)
    {
        try
        {
            File.Delete(_scripts.Inverse[script]);
            return _scripts.Inverse.Remove(script);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            return false;
        }
    }

    public override bool Remove(string source)
    {
        File.Delete(source);
        return _scripts.Remove(source);
    }

    public override void Update(Script script)
    {
        if (!Contains(script))
        {
            throw new InvalidOperationException("The script is not present in the repository");
        }
        var source = _scripts.Inverse[script];
        try
        {
            using Stream file = File.Open(source, FileMode.Create, FileAccess.Write);
            Serializer.Serialize(script, file);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, FSVerb.Access, source);
        }
    }

    protected override void Clear() => _scripts.Clear();

    protected override void Load()
    {
        foreach (var scriptFile in Directory.EnumerateFiles(_directory, '*' + _scriptFileExtension, SearchOption.AllDirectories))
        {
            bool retry;
            do
            {
                try
                {
                    _ = Load(scriptFile);
                    retry = false;
                }
                catch (FileSystemException e)
                {
                    retry = _fsErrorReloadElseIgnore(e);
                }
                catch (DeserializationException e)
                {
                    var action = _scriptLoadError(e, scriptFile);
                    if (action is InvalidScriptDataAction.Remove)
                    {
                        File.Delete(scriptFile);
                    }
                    retry = action is InvalidScriptDataAction.Reload;
                }
            } while (retry);
        }
    }

    private void Add(string sourceFilename, Script script)
    {
        string savingPath = Path.Join(_directory, Path.ChangeExtension(sourceFilename, _scriptFileExtension));
        try
        {
            _scripts.Add(savingPath, script);
        }
        catch (ArgumentException e)
        {
            throw new ScriptAlreadyExistsException(script, e);
        }
        using Stream file = File.Create(savingPath);
        Serializer.Serialize(script, file);
    }

    private Script Load(string source)
    {
        try
        {
            using Stream file = File.OpenRead(source);
            var script = Serializer.Deserialize(Type, file);
            _scripts.Add(source, script);
            return script;
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, FSVerb.Access, source);
        }
    }
}