using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts created from files.</summary>
public sealed class FileScriptCollection : ScriptCollection, IMutableScriptCollection
{
    private readonly string _directory;

    public FileScriptCollection(string directory, string scriptFileExtension,
        InvalidScriptDataCallback invalidScriptDataReloadElseIgnore, FSErrorCallback fsErrorReloadElseIgnore,
        IScriptSerializer serializer, ScriptType scriptType) : base(serializer, scriptType)
    {
        _directory = directory;
        foreach (var filePath in Directory.EnumerateFiles(directory, $"*{scriptFileExtension}",
                     SearchOption.AllDirectories))
        {
        retry:
            try
            {
                Load(filePath);
            }
            catch (Exception e) when (e.IsFileSystem())
            {
                if (fsErrorReloadElseIgnore(e, FSVerb.Access, new FileInfo(filePath)))
                {
                    goto retry;
                }
            }
            catch (InvalidDataException e)
            {
                if (invalidScriptDataReloadElseIgnore(e, filePath))
                {
                    goto retry;
                }
            }
        }
    }

    public void Add(Script script)
    {
        string savingPath = Path.Join(_directory, script.InvariantName.ToFilename());
        try
        {
            using Stream file = File.Open(savingPath, FileMode.CreateNew);
            Serialize(script, file);
        }
        catch (IOException e) when (e.HResult == -2147024816) // 0x80070050: The file already exists
        {
            using Stream file = File.OpenRead(savingPath);
            throw new ScriptAlreadyExistsException(Deserialize(file), e);
        }
        Sources.Add(script, savingPath);
    }

    public void Remove(Script script)
    {
        File.Delete(Sources[script]);
        _ = Sources.Remove(script);
    }

    public void Save()
    {
        foreach (Script s in this)
        {
            using Stream FILE = File.Open(Sources[s], FileMode.Create, FileAccess.Write);
            Serialize(s, FILE);
        }
    }

    protected override void Load(string source)
    {
        using Stream file = File.OpenRead(source);
        var script = Deserialize(file);
        Sources.Add(script, source);
    }
}