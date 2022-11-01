namespace Scover.WinClean.BusinessLogic.Scripts;

public interface IScriptSerializer
{
    /// <summary>Deserializes a script from the provided <see cref="data"/>.</summary>
    /// <returns>A new <see cref="Script"/> object with its property <see cref="Script.IsDefault"/> set to <see langword="false"/>.</returns>
    /// <exception cref="InvalidDataException">
    /// The script could not be deserialized because <paramref name="data"/> is incomplete or in an invalid format.
    /// </exception>
    Script Deserialize(Stream data);

    /// <summary>Deserializes a default script from the provided <see cref="data"/>.</summary>
    /// <returns>A new <see cref="Script"/> object with its property <see cref="Script.IsDefault"/> set to <see langword="true"/>.</returns>
    /// <exception cref="InvalidDataException">
    /// The script could not be deserialized because <paramref name="data"/> is incomplete or in an invalid format.
    /// </exception>
    Script DeserializeDefault(Stream data);

    /// <summary>Serializes a script.</summary>
    void Serialize(Script script, Stream stream);
}