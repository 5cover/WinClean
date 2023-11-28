using System.Windows.Media;

namespace Scover.WinClean.Model.Metadatas;

public sealed class SafetyLevel : OrderedMetadata
{
    /// <param name="color">The color of the safety level.</param>
    public SafetyLevel(LocalizedString name, LocalizedString description, int order, Color color) : base(new LocalizedStringTextProvider(name, description), order)
        => Color = color;

    public Color Color { get; }
}