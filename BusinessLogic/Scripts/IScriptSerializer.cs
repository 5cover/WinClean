namespace Scover.WinClean.BusinessLogic.Scripts;

public interface IScriptSerializer
{
    /// <summary>Deserializes a script from the provided <see cref="data"/>.</summary>
    /// <returns>A new <see cref="Script"/> object.</returns>
    /// <exception cref="InvalidDataException">
    /// The script could not be deserialized because <paramref name="data"/> is incomplete or in an invalid format.
    /// </exception>
    Script Deserialize(Stream data);

    /// <summary>Serializes a script.</summary>
    void Serialize(Script script, Stream stream);
}