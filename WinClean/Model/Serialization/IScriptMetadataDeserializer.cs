using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model.Serialization;

public interface IScriptMetadataDeserializer
{
    /// <summary>Deserializes a collection of categories from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="DeserializationException">
    /// <paramref name="stream"/> is in a invalid or incomplete format.
    /// </exception>
    /// <returns>A collection of deserialized <see cref="Category"/> objects.</returns>
    IEnumerable<Category> GetCategories(Stream stream);

    /// <summary>Deserializes a collection of hosts from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="DeserializationException">
    /// <paramref name="stream"/> is in a invalid or incomplete format.
    /// </exception>
    /// <returns>A collection of deserialized <see cref="Host"/> objects.</returns>
    IEnumerable<Host> GetHosts(Stream stream);

    /// <summary>Deserializes a collection of impacts from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="DeserializationException">
    /// <paramref name="stream"/> is in a invalid or incomplete format.
    /// </exception>
    /// <returns>A collection of deserialized <see cref="Impact"/> objects.</returns>
    IEnumerable<Impact> GetImpacts(Stream stream);

    /// <summary>Deserializes a collection of safety levels from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="DeserializationException">
    /// <paramref name="stream"/> is in a invalid or incomplete format.
    /// </exception>
    /// <returns>A collection of deserialized <see cref="SafetyLevel"/> objects.</returns>
    IEnumerable<SafetyLevel> GetSafetyLevels(Stream stream);
}