namespace Scover.WinClean.Model.Metadatas;

/// <summary>Effect of running a script.</summary>
public sealed class Impact : Metadata
{
    public Impact(LocalizedString name, LocalizedString description) : base(new LocalizedStringTextProvider(name, description))
    {
    }
}