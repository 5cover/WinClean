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

    public FileScriptRepository(string directory,
                                string scriptFileExtension,
                                ScriptDeserializationErrorCallback scriptLoadError,
                                FSErrorCallback fsErrorReloadElseIgnore,
                                IScriptSerializer serializer,
                                ScriptType type) : base(serializer, type)
        => (_directory, _scriptFileExtension, _scriptLoadError, _fsErrorReloadElseIgnore)
            = (directory, scriptFileExtension, scriptLoadError, fsErrorReloadElseIgnore);

    public override void Commit(Script script)
    {
        string savingPath = script.Source;
        if (!File.Exists(savingPath))
        {
            throw new ArgumentException(ExceptionMessages.ScriptNotInRepo);
        }
        WriteScriptFile(script, savingPath);
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
                        scriptFile.PerformFileSystemOperation(File.Delete, FSVerb.Delete);
                    }
                    retry = action is InvalidScriptDataAction.Reload;
                }
            } while (retry);
        }
    }

    public override Script RetrieveScript(string source)
    {
        try
        {
            using var stream = File.OpenRead(source);
            return Serializer.Deserialize(stream).Complete(Type, source);
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

    protected override void Add(Script script)
    {
        // This is the only method where script is not supposed to already exist in the the repository,
        // hence the transformation of script.Source which points to an external resource.
        string savingPath = Path.Join(_directory, Path.ChangeExtension(Path.GetFileName(script.Source), _scriptFileExtension));
        if (File.Exists(savingPath))
        {
            throw new ScriptAlreadyExistsException(script);
        }
        WriteScriptFile(script, savingPath);
    }

    protected override bool Remove(Script script)
    {
        try
        {
            File.Delete(script.Source);
        }
        catch (Exception e) when (e.IsFileSystemExogenous())
        {
            return false;
        }
        return true;
    }

    private async Task LoadAsync(string source)
    {
        string fileContents = await source.PerformFileSystemOperation(path => File.ReadAllTextAsync(path), FSVerb.Access);
        AddItemQuietly(Serializer.Deserialize(fileContents).Complete(Type, source));
    }

    private void WriteScriptFile(Script script, string path)
    {
        using Stream file = path.PerformFileSystemOperation(p => File.Open(p, FileMode.Create, FileAccess.Write), FSVerb.Create);
        Serializer.Serialize(script, file);
    }
}