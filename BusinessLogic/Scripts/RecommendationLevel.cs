using System.Windows.Media;

namespace Scover.WinClean.BusinessLogic.Scripts;

public class RecommendationLevel : LocalizableScriptMetadata
{
    public RecommendationLevel(LocalizedString names, LocalizedString descriptions, Color color) : base(names, descriptions)
        => Color = color;

    public Color Color { get; }
}