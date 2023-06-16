namespace Scover.WinClean.Model.Metadatas;

public sealed class Category : Metadata
{
    private readonly int _order;

    public Category(LocalizedString name, LocalizedString description, int order) : base(new LocalizedStringTextProvider(name, description))
        => _order = order;

    public override int CompareTo(Metadata? other) => _order.CompareTo((other as Category)?._order);
}