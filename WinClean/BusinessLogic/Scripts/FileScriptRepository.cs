namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class FileScriptRepository : MutableScriptRepository
{
    private readonly string _directory;
    private readonly FSErrorCallback _fsErrorReloadElseIgnore;
    private readonly InvalidScriptDataCallback _invalidScriptData;
    private readonly string _scriptFileExtension;
    private readonly IScriptSerializer _serializer;
    private readonly BidirectionalDictionary<string, Script> _scripts = new();

    public FileScriptRepository(ScriptType type,
                                string directory,
                                string scriptFileExtension,
                                IScriptSerializer serializer,
                                InvalidScriptDataCallback invalidScriptData,
                                FSErrorCallback fsErrorReloadElseIgnore) : base(type)
        => (_directory, _scriptFileExtension, _serializer, _invalidScriptData, _fsErrorReloadElseIgnore)
            = (directory, scriptFileExtension, serializer, invalidScriptData, fsErrorReloadElseIgnore);

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

    public override Script Add(string source)
    {
        try
        {
            using Stream file = File.OpenRead(source);
            var script = _serializer.Deserialize(Type, file);
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
                catch (InvalidDataException e)
                {
                    var action = _invalidScriptData(e, filePath);
                    if (action is InvalidScriptDataAction.Remove)
                    {
                        File.Delete(filePath);
                    }
                    retry = action is InvalidScriptDataAction.Reload;
                }
            }
        }
    }

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
            _serializer.Serialize(script, file);
        }
    }
}