using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;

namespace Scover.WinClean.Model.Serialization;

public interface IScriptSerializer
{
    /// <summary>Deserializes a script from the provided <paramref name="data"/>.</summary>
    /// <param name="data">A stream with read access.</param>
    /// <param name="type">The type of the script to create.</param>
    /// <returns>A new <see cref="IScript"/> object.</returns>
    /// <exception cref="DeserializationException">
    /// The script could not be deserialized because <paramref name="data"/> is in an incomplete or invalid
    /// format.
    /// </exception>
    Script Deserialize(ScriptType type, Stream data);

    /// <summary>Deserializes a script from the provided <paramref name="data"/>.</summary>
    /// <param name="data">A string containing the data.</param>
    /// <param name="type">The type of the script to create.</param>
    /// <returns>A new <see cref="IScript"/> object.</returns>
    /// <exception cref="DeserializationException">
    /// The script could not be deserialized because <paramref name="data"/> is in an incomplete or invalid
    /// format.
    /// </exception>
    Script Deserialize(ScriptType type, string data);

    /// <summary>Serializes a script to a stream.</summary>
    /// <param name="script">The script to serialize.</param>
    /// <param name="stream">A stream with write access.</param>
    void Serialize(Script script, Stream stream);
}