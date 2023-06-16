using System.Windows.Media;

namespace Scover.WinClean.Model.Metadatas;

public sealed class SafetyLevel : Metadata
{
    private readonly int _order;

    /// <param name="color">The color of the safety level.</param>
    public SafetyLevel(LocalizedString name, LocalizedString description, int order, Color color) : base(new LocalizedStringTextProvider(name, description))
        => (Color, _order) = (color, order);

    public Color Color { get; }

    public override int CompareTo(Metadata? other) => _order.CompareTo((other as SafetyLevel)?._order);
}