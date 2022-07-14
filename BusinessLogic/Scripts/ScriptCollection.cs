using System.Collections;

using Scover.WinClean.BusinessLogic.Xml;

namespace Scover.WinClean.BusinessLogic.Scripts;

public class ScriptCollection : IReadOnlyCollection<Script>
{
    private static readonly IScriptSerializer serializer = new ScriptXmlSerializer();
    private readonly Dictionary<Script, string> _scriptFiles = new();
    public int Count => EnumerateScriptFiles().Count();

    /// <summary>Loads all the scripts present in the scripts directory.</summary>
    /// <param name="reloadOnInvalidScriptData"><inheritdoc cref="InvalidScriptDataCallback" path="/summary"/> Returns <see langword="true"/> if the script should be reloaded, <see langword="false"/> if it should be ignored.</param>
    /// <remarks>Will not load scripts located in subdirectories.</remarks>
    public static ScriptCollection LoadScripts(InvalidScriptDataCallback reloadOnInvalidScriptData)
    {
        ScriptCollection scripts = new();

        foreach (string scriptFile in EnumerateScriptFiles())
        {
            Script? deserializedScript = Deserialize(scriptFile, reloadOnInvalidScriptData);
            if (deserializedScript is not null)
            {
                scripts.AddToDictionary(deserializedScript, scriptFile);
            }
        }

        return scripts;
    }

    /// <summary>Loads a script and adds it to the collection. Also copies <paramref name="sourceFile"/> to the scripts directory.</summary>
    /// <param name="sourceFile">The path to the script file.</param>
    /// <exception cref="InvalidDataException">
    /// The deserialization process failed because <paramref name="sourceFile"/> has invalid or missing data.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="allowOverwrite"/> is <see langword="true"/> and <paramref name="sourceFile"/> already exists in the
    /// scripts directory.
    /// </exception>
    public void AddNew(string sourceFile, bool allowOverwrite)
    {
        // 1. Try to deserialize the script. This line will throw an exception if sourceFile contains invalid data or can't be read.
        using Stream stream = File.OpenRead(sourceFile);
        Script script = serializer.Deserialize(stream);

        // 2. Try to copy the script file to the scripts directory. If overwrite is false and the destination file already
        // exists, this will throw.
        CopyFile(sourceFile, GetDestFilePath(sourceFile), allowOverwrite);

        // 3. Reaching this line means that there was no error, so the new script may be added to the dictionary.
        AddToDictionary(script, sourceFile);
    }

    public IEnumerator<Script> GetEnumerator() => _scriptFiles.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _scriptFiles.Keys.GetEnumerator();

    /// <summary>Removes a script from the collection. Also deletes its corresponding file in the scripts directory.</summary>
    /// <param name="item">The script to remove.</param>
    public void Remove(Script item)
    {
        File.Delete(_scriptFiles[item]);
        _ = _scriptFiles.Remove(item);
    }

    /// <summary>Saves the scripts by serializing them to the scripts directory.</summary>
    public void Save()
    {
        foreach (Script script in this)
        {
            // truncate the file, so that if the xml is shorter than last time there won't be remains of the old version.
            using Stream stream = File.Open(_scriptFiles[script], FileMode.Truncate, FileAccess.Write);
            serializer.Serialize(script, stream);
        }
    }

    private static void CopyFile(string source, string dest, bool overwrite)
    {
        try
        {
            File.Copy(source, dest, overwrite);
        }
        catch (IOException e) when (e.HResult == -2147024816)// 0x80070050 : The file already exists
        {
            throw new InvalidOperationException($"The file \"{dest}\" already exists.", e);
        }
    }

    private static Script? Deserialize(string scriptFile, InvalidScriptDataCallback reloadOnInvalidScriptData)
    {
        while (true)
        {
            try
            {
                using Stream stream = File.OpenRead(scriptFile);
                return serializer.Deserialize(stream);
            }
            catch (InvalidDataException e)
            {
                if (!reloadOnInvalidScriptData(e, scriptFile))
                {
                    return null;
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateScriptFiles() => Directory.EnumerateFiles(AppDirectory.ScriptsDir.Info.FullName, "*.xml", SearchOption.TopDirectoryOnly);

    private static string GetDestFilePath(string sourceFile) => AppDirectory.ScriptsDir.Join(Path.GetFileName(sourceFile));

    private void AddToDictionary(Script item, string sourceFilePath)
            => _scriptFiles.Add(item, GetDestFilePath(sourceFilePath));
}