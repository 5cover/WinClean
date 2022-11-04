namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class Category : ScriptMetadata
{
    /// <summary>Initializes a new <see cref="Category"/> object.</summary>
    /// <inheritdoc cref="ScriptMetadata(LocalizedString, LocalizedString)" path="/param"/>
    public Category(LocalizedString name, LocalizedString description) : base(name, description)
    {
    }
}