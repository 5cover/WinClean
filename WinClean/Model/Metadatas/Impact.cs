namespace Scover.WinClean.Model.Metadatas;

/// <summary>Effect of running a script.</summary>
public sealed record Impact : ScriptLocalizedStringMetadata
{
    public Impact(LocalizedString name, LocalizedString description) : base(name, description)
    {
    }
}