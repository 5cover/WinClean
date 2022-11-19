using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Effect of running a script.</summary>
public sealed record Impact : ScriptMetadata
{
    public Impact(LocalizedString name, LocalizedString description) : base(name, description)
    {
    }
}