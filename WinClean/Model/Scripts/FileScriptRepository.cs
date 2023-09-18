using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Resources;

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

    public override void Commit(Script script)
    {
        if (!_scripts.ContainsValue(script))
        {
            throw new InvalidOperationException(ExceptionMessages.ScriptNotInRepo);
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

    public override Script GetScript(string source)
    {
        try
        {
            using var stream = File.OpenRead(source);
            return Serializer.Deserialize(Type, stream);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new ArgumentException(ExceptionMessages.ScriptNotFoundAtSource.FormatWith(source), nameof(source), e);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, FSVerb.Access, source);
        }
    }

    public override async Task LoadAsync()
    {
        foreach (var scriptFile in Directory.EnumerateFiles(_directory, '*' + _scriptFileExtension, SearchOption.AllDirectories))
        {
            bool retry;
            do
            {
                try
                {
                    await LoadAsync(scriptFile);
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

    public override bool Remove(Script script)
    {
        try
        {
            File.Delete(_scripts.Inverse[script]);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            return false;
        }
        _ = RemoveItem(script);
        return _scripts.Inverse.Remove(script);
    }

    public override bool Remove(string source)
    {
        File.Delete(source);
        _ = RemoveItem(_scripts[source]);
        return _scripts.Remove(source);
    }

    private void Add(string sourceFilename, Script script)
    {
        string savingPath = Path.Join(_directory, Path.ChangeExtension(sourceFilename, _scriptFileExtension));
        try
        {
            _scripts.Add(savingPath, script);
            using Stream file = File.Create(savingPath);
            Serializer.Serialize(script, file);
        }
        catch (ArgumentException e)
        {
            throw new ScriptAlreadyExistsException(script, e);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, FSVerb.Access, savingPath);
        }
        AddItem(script);
    }

    private async Task LoadAsync(string source)
    {
        try
        {
            var script = Serializer.Deserialize(Type, await File.ReadAllTextAsync(source));
            _scripts.Add(source, script);
            AddItem(script);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            throw new FileSystemException(e, FSVerb.Access, source);
        }
    }
}