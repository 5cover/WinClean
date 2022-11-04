namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>Effect of running a script.</summary>
public sealed class Impact : ScriptMetadata
{
    /// <summary>Initializes a new <see cref="Impact"/> object.</summary>
    /// <inheritdoc cref="ScriptMetadata(LocalizedString, LocalizedString)" path="/param"/>
    public Impact(LocalizedString name, LocalizedString description) : base(name, description)
    {
    }
}