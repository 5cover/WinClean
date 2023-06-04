namespace Scover.WinClean.Model.Metadatas;

public sealed record Category : ScriptLocalizedStringMetadata
{
    public Category(LocalizedString name, LocalizedString description) : base(name, description)
    {
    }
}