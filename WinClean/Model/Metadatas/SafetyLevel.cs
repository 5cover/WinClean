using System.Windows.Media;

namespace Scover.WinClean.Model.Metadatas;

public sealed record SafetyLevel : ScriptLocalizedStringMetadata
{
    /// <param name="color">The color of the safety level.</param>
    public SafetyLevel(LocalizedString name, LocalizedString description, Color color) : base(name, description)
        => Color = color;

    public Color Color { get; }
}