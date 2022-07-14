using Scover.WinClean.DataAccess;
using Scover.WinClean.Resources;

namespace Scover.WinClean.BusinessLogic.Scripts;

public static class ScriptMetadataFactory
{
    /// <summary>Gets the <see cref="Category"/> object with this invariant name.</summary>
    /// <exception cref="ArgumentException">The invariant name is not a valid <see cref="Category"/> invariant name.</exception>
    public static Category GetCategory(string invariantName) => FromInvariantName(AppInfo.Categories, invariantName);

    /// <summary>Gets the <see cref="Host"/> object with this invariant name.</summary>
    /// <exception cref="ArgumentException">The invariant name is not a valid <see cref="Host"/> invariant name.</exception>
    public static Host GetHost(string invariantName) => FromInvariantName(AppInfo.Hosts, invariantName);

    /// <summary>Gets the <see cref="Impact"/> object with this invariant name.</summary>
    /// <exception cref="ArgumentException">The invariant name is not a valid <see cref="Impact"/> invariant name.</exception>
    public static Impact GetImpact(string invariantName) => FromInvariantName(AppInfo.Impacts, invariantName);

    /// <summary>Gets the <see cref="RecommendationLevel"/> object with this invariant name.</summary>
    /// <exception cref="ArgumentException">
    /// The invariant name is not a valid <see cref="RecommendationLevel"/> invariant name.
    /// </exception>
    public static RecommendationLevel GetRecommendationLevel(string invariantName) => FromInvariantName(AppInfo.RecommendationLevels, invariantName);

    private static T FromInvariantName<T>(IEnumerable<T> values, string invariantName) where T : IUserVisible
        => values.SingleOrDefault(value => value.InvariantName == invariantName)
        ?? throw new ArgumentException(DevException.NotAValid.FormatWith(invariantName, typeof(T).Name, nameof(IUserVisible.InvariantName)), nameof(invariantName));
}