using System.Collections;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts that interacts with a storage system.</summary>
public abstract class ScriptCollection : IEnumerable<Script>
{
    private readonly ScriptType _scriptType;

    private readonly IScriptSerializer _serializer;

    /// <param name="serializer">The serializer to use to serialize and deserialize scripts.</param>
    /// <param name="scriptType">The type of the scripts being stored.</param>
    protected ScriptCollection(IScriptSerializer serializer, ScriptType scriptType)
                => (_serializer, _scriptType) = (serializer, scriptType);

    protected Dictionary<Script, string> Sources { get; } = new();

    public IEnumerator<Script> GetEnumerator() => Sources.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Sources.Keys.GetEnumerator();

    /// <inheritdoc cref="IScriptSerializer.Deserialize(ScriptType, Stream)"/>
    protected Script Deserialize(Stream data) => _serializer.Deserialize(_scriptType, data);

    /// <summary>Loads the script at <paramref name="source"/> in the storage system.</summary>
    /// <param name="source">A string that identifies the script to load in the storage system.</param>
    /// <returns>The script that was loaded.</returns>
    /// <exception cref="ScriptNotFoundException">The script at <paramref name="source"/> was not found.</exception>
    /// <exception cref="InvalidDataException">
    /// The deserialization failed because <paramref name="source"/> is in a invalid or incomplete format.
    /// </exception>
    protected abstract void Load(string source);

    /// <inheritdoc cref="IScriptSerializer.Serialize(Script, Stream)"/>
    protected void Serialize(Script script, Stream stream) => _serializer.Serialize(script, stream);
}