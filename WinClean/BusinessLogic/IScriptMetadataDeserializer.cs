namespace Scover.WinClean.BusinessLogic;

public interface IScriptMetadataDeserializer
{
    /// <summary>Deserializes a collection of categories from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> is in a invalid or incomplete format.</exception>
    /// <returns>A collection of deserialized <see cref="Category"/> objects.</returns>
    IEnumerable<Category> GetCategories(Stream stream);

    /// <summary>Deserializes a collection of hosts from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> is in a invalid or incomplete format.</exception>
    /// <returns>A collection of deserialized <see cref="Host"/> objects.</returns>
    IEnumerable<Host> GetHosts(Stream stream);

    /// <summary>Deserializes a collection of impacts from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> is in a invalid or incomplete format.</exception>
    /// <returns>A collection of deserialized <see cref="Impact"/> objects.</returns>
    IEnumerable<Impact> GetImpacts(Stream stream);

    /// <summary>Deserializes a collection of recommendation levels from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> is in a invalid or incomplete format.</exception>
    /// <returns>A collection of deserialized <see cref="RecommendationLevel"/> objects.</returns>
    IEnumerable<RecommendationLevel> GetRecommendationLevels(Stream stream);
}