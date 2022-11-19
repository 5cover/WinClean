using System.Collections;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts that interacts with a storage system.</summary>
public abstract class ScriptCollection : IEnumerable<Script>
{
    private readonly ScriptType _scriptType;

    private readonly IScriptSerializer _serializer;

    protected ScriptCollection(IScriptSerializer serializer, ScriptType scriptType)
                => (_serializer, _scriptType) = (serializer, scriptType);

    protected Dictionary<Script, string> Sources { get; } = new();

    public IEnumerator<Script> GetEnumerator() => Sources.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Sources.Keys.GetEnumerator();

    /// <summary>Loads the script at <paramref name="source"/> in the storage system.</summary>
    /// <param name="source">A string that identifies the script to load in the storage system.</param>
    /// <returns>The script that was loaded.</returns>
    /// <exception cref="ScriptNotFoundException">The script at <paramref name="source"/> was not found.</exception>
    /// <exception cref="InvalidDataException">
    /// The deserialization failed because <paramref name="source"/> is in a invalid or incomplete format.
    /// </exception>
    public abstract void Load(string source);

    /// <summary>Loads all scripts in <paramref name="sources"/> from the storage system.</summary>
    /// <param name="sources">The source strings that identify the scripts to load.</param>
    /// <param name="reloadElseIgnore">
    /// <inheritdoc cref="InvalidScriptDataCallback" path="/summary"/> Returns <see langword="true"/> if the script should be
    /// reloaded, <see langword="false"/> if it should be ignored.
    /// </param>
    /// <inheritdoc cref="Load(string)" path="/exception"/>
    public void LoadAll(IEnumerable<string> sources, InvalidScriptDataCallback? reloadElseIgnore = null)
    {
        foreach (var source in sources)
        {
        retry:
            try
            {
                Load(source);
            }
            catch (InvalidDataException e)
            {
                if (reloadElseIgnore is null)
                {
                    throw;
                }
                if (reloadElseIgnore.Invoke(e, source))
                {
                    goto retry;
                }
            }
        }
    }

    /// <inheritdoc cref="IScriptSerializer.Deserialize(ScriptType, Stream)"/>
    protected Script Deserialize(Stream data) => _serializer.Deserialize(_scriptType, data);

    /// <inheritdoc cref="IScriptSerializer.Serialize(Script, Stream)"/>
    protected void Serialize(Script script, Stream stream) => _serializer.Serialize(script, stream);
}