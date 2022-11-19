using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts created from files.</summary>
public sealed class FileScriptCollection : ScriptCollection, IMutableScriptCollection
{
    private readonly string _directory;

    public FileScriptCollection(string directory, IScriptSerializer serializer, ScriptType scriptType) : base(serializer, scriptType)
            => _directory = directory;

    public Script Add(string source)
    {
        if (source.IsPathInDirectory(source))
        {
            throw new ArgumentException($"The path '{source}' is already in the storage directory '{_directory}'", nameof(source));
        }

        using Stream sourceFile = File.OpenRead(source);
        var script = Deserialize(sourceFile);
        AddAndCopy(script, Path.Join(_directory, Path.GetFileName(source)));
        return script;
    }

    public void Add(Script script)
        => AddAndCopy(script, Path.Join(_directory, script.InvariantName.ToFilename()));

    /// <param name="source">The path to the script file.</param>
    /// <inheritdoc/>
    /// <exception cref="PathTooLongException">
    /// <paramref name="source"/>'s path, file name, or both exceed the system-defined maximum length.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// <paramref name="source"/> is invalid, (for example, it is on an unmapped drive).
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">
    /// <paramref name="source"/> specified a directory. -or- The caller does not have the required permission.
    /// </exception>
    /// <exception cref="NotSupportedException"><paramref name="source"/> is in an invalid format.</exception>
    /// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
    public override void Load(string source)
    {
        try
        {
            using Stream file = File.OpenRead(source);
            var script = Deserialize(file);
            Sources.Add(script, source);
        }
        catch (FileNotFoundException e)
        {
            throw new ScriptNotFoundException(source, e);
        }
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

    private void AddAndCopy(Script script, string savingPath)
    {
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
}