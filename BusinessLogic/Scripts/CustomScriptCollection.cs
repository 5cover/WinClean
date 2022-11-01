using System.Collections;
using System.Reflection;

using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts deserialized from files.</summary>
public sealed class CustomScriptCollection : IEnumerable<Script>
{
    private static readonly IScriptSerializer serializer = new ScriptXmlSerializer();
    private readonly Dictionary<Script, string> _scriptLocations = new();

    /// <summary>Loads all the scripts present in the specified application directory.</summary>
    /// <param name="directory">The application directory to load the scripts from.</param>
    /// <param name="reloadElseIgnore">
    /// <inheritdoc cref="InvalidScriptDataCallback" path="/summary"/> Returns <see langword="true"/> if the script should be
    /// reloaded, <see langword="false"/> if it should be ignored.
    /// </param>
    /// <remarks>Will not load scripts located in subdirectories.</remarks>
    public static CustomScriptCollection LoadScripts(AppDirectory directory, InvalidScriptDataCallback reloadElseIgnore)
    {
        CustomScriptCollection scripts = new();
        foreach (string scriptFile in Directory.EnumerateFiles(directory.Info.FullName, AppInfo.Settings.ScriptFileExtension, SearchOption.TopDirectoryOnly))
        {
            Script? deserializedScript = DeserializeTolerantly(scriptFile, reloadElseIgnore);
            if (deserializedScript is not null)
            {
                scripts._scriptLocations.Add(deserializedScript, scriptFile);
            }
        }

        return scripts;
    }

    /// <summary>Loads a script and adds it to the collection. Also copies <paramref name="sourceFile"/> to the scripts directory.</summary>
    /// <param name="sourceFile">The path to the script file.</param>
    /// <exception cref="InvalidDataException">
    /// The deserialization process failed because <paramref name="sourceFile"/> has invalid or missing data.
    /// </exception>
    /// <exception cref="ScriptAlreadyExistsException">
    /// <paramref name="allowOverwrite"/> is <see langword="false"/> and <paramref name="sourceFile"/> already exists in the
    /// scripts directory.
    /// </exception>
    /// <inheritdoc cref="File.OpenRead(string)" path="/exception"/>
    public void AddNew(string sourceFile, bool allowOverwrite)
    {
        // 1. Try to deserialize the script. This line will throw an exception if sourceFile contains invalid data or can't be read.
        using Stream stream = File.OpenRead(sourceFile);
        var script = serializer.Deserialize(stream);

        string savingPath = AppDirectory.ScriptsDir.Join(Path.GetFileName(sourceFile));

        // 2. Try to copy the script file to the scripts directory.
        try
        {
            File.Copy(sourceFile, savingPath, allowOverwrite);
        }
        catch (IOException e) when (e.HResult == -2147024816) // 0x80070050 : The file already exists
        {
            throw new ScriptAlreadyExistsException(serializer.Deserialize(File.OpenRead(savingPath)), e);
        }

        // 3. Reaching this line means that there was no error, so the new script may be added to the dictionary.
        _scriptLocations.Add(script, savingPath);
    }

    public IEnumerator<Script> GetEnumerator() => _scriptLocations.Keys.GetEnumerator();

    /// <summary>Removes a script from the collection. Also deletes its corresponding file in the scripts directory.</summary>
    /// <param name="item">The script to remove.</param>
    public void Remove(Script item)
    {
        File.Delete(_scriptLocations[item]);
        _ = _scriptLocations.Remove(item);
    }

    /// <summary>Saves the scripts by serializing them to the scripts directory.</summary>
    public void Save()
    {
        foreach (Script s in this)
        {
            // Truncate the file, so that if the xml is shorter than last time there won't be remains of the old version.
            using Stream stream = File.Open(_scriptLocations[s], FileMode.Truncate, FileAccess.Write);
            serializer.Serialize(s, stream);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => _scriptLocations.Keys.GetEnumerator();

    private static Script? DeserializeTolerantly(string scriptFile, InvalidScriptDataCallback reloadElseReturnNull)
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
                if (!reloadElseReturnNull(e, scriptFile))
                {
                    return null;
                }
            }
            catch (Exception e) when (e.IsFileSystem())
            {
                // Fail silently and skip loading the script if it cannot be loaded due to a filesystem errro.
                return null;
            }
        }
    }
}