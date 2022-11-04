namespace Scover.WinClean.BusinessLogic.Scripts;

public interface IScriptMetadataDeserializer
{
    /// <summary>Deserializes a collection of categories from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> points to invalid data.</exception>
    /// <returns>A collection of deserialized <see cref="Category"/> objects.</returns>
    IEnumerable<Category> MakeCategories(Stream stream);

    /// <summary>Deserializes a collection of hosts from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> points to invalid data.</exception>
    /// <returns>A collection of deserialized <see cref="Host"/> objects.</returns>
    IEnumerable<Host> MakeHosts(Stream stream);

    /// <summary>Deserializes a collection of impacts from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> points to invalid data.</exception>
    /// <returns>A collection of deserialized <see cref="Impact"/> objects.</returns>
    IEnumerable<Impact> MakeImpacts(Stream stream);

    /// <summary>Deserializes a collection of recommendation levels from the specified stream.</summary>
    /// <param name="stream">A stream with read access.</param>
    /// <exception cref="InvalidDataException"><paramref name="stream"/> points to invalid data.</exception>
    /// <returns>A collection of deserialized <see cref="RecommendationLevel"/> objects.</returns>
    IEnumerable<RecommendationLevel> MakeRecommendationLevels(Stream stream);
}