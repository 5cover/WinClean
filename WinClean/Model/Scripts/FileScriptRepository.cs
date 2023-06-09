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

    public override void Add(Script script)
    {
        string savingPath = Path.Join(_directory, script.InvariantName.ToFilename() + _scriptFileExtension);
        try
        {
            _scripts.Add(savingPath, script);
        }
        catch (ArgumentException e)
        {
            throw new ScriptAlreadyExistsException(script, e);
        }
    }

    /// <exception cref="DeserializationException"/>
    /// <exception cref="FileSystemException"/>
    /// <exception cref="ScriptAlreadyExistsException"/>
    public override Script Add(string source)
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
        catch (ArgumentException e)
        {
            throw new ScriptAlreadyExistsException(_scripts[source], e);
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

    public override void Save()
    {
        foreach (Script script in this)
        {
            using Stream file = File.Create(_scripts.Inverse[script]);
            Serializer.Serialize(script, file);
        }
    }

    protected override void LoadScripts()
    {
        foreach (var filePath in Directory.EnumerateFiles(_directory, '*' + _scriptFileExtension, SearchOption.AllDirectories))
        {
            bool retry = true;
            while (retry)
            {
                try
                {
                    _ = Add(filePath);
                    retry = false;
                }
                catch (FileSystemException e)
                {
                    retry = _fsErrorReloadElseIgnore(e);
                }
                catch (DeserializationException e)
                {
                    var action = _scriptLoadError(e, filePath);
                    if (action is InvalidScriptDataAction.Remove)
                    {
                        File.Delete(filePath);
                    }
                    retry = action is InvalidScriptDataAction.Reload;
                }
            }
        }
    }
}