using System.Windows.Media;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

public sealed record RecommendationLevel : ScriptMetadata
{
    /// <param name="color">The color of the recommendation level.</param>
    public RecommendationLevel(LocalizedString name, LocalizedString description, Color color) : base(name, description)
        => Color = color;

    public Color Color { get; }
}