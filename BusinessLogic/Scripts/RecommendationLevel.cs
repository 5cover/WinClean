using System.Windows.Media;

namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class RecommendationLevel : ScriptMetadata
{
    /// <summary>Initializes a new <see cref="RecommendationLevel"/> object.</summary>
    /// <inheritdoc cref="ScriptMetadata(LocalizedString, LocalizedString)" path="/param"/>
    /// <param name="color">The color of the recommendation level</param>
    public RecommendationLevel(LocalizedString name, LocalizedString description, Color color) : base(name, description)
        => Color = color;

    public Color Color { get; }
}