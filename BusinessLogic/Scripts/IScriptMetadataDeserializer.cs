namespace Scover.WinClean.BusinessLogic.Scripts;

public interface IScriptMetadataDeserializer
{
    IEnumerable<Category> MakeCategories();

    IEnumerable<Impact> MakeImpacts();

    IEnumerable<RecommendationLevel> MakeRecommendationLevels();
}