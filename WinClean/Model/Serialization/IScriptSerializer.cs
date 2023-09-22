using Scover.WinClean.Model.Scripts;

namespace Scover.WinClean.Model.Serialization;

public interface IScriptSerializer
{
    /// <summary>Deserializes a script from the provided <paramref name="data"/>.</summary>
    /// <param name="data">A stream with read access.</param>
    /// <returns>A script builder object that can be used to build the final script.</returns>
    /// <exception cref="DeserializationException">
    /// The script could not be deserialized because <paramref name="data"/> is in an incomplete or invalid
    /// format.
    /// </exception>
    ScriptBuilder Deserialize(Stream data);

    /// <summary>Deserializes a script from the provided <paramref name="data"/>.</summary>
    /// <param name="data">A string containing the data.</param>
    /// <returns>A script builder object that can be used to build the final script.</returns>
    /// <exception cref="DeserializationException">
    /// The script could not be deserialized because <paramref name="data"/> is in an incomplete or invalid
    /// format.
    /// </exception>
    ScriptBuilder Deserialize(string data);

    /// <summary>Serializes a script to a stream.</summary>
    /// <param name="script">The script to serialize.</param>
    /// <param name="stream">A stream with write access.</param>
    void Serialize(Script script, Stream stream);
}