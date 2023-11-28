namespace Scover.WinClean.Model.Metadatas;

public sealed class Category : OrderedMetadata
{
    public Category(LocalizedString name, LocalizedString description, int order) : base(new LocalizedStringTextProvider(name, description), order)
    {
    }
}