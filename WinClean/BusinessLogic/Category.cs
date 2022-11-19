using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

public sealed record Category : ScriptMetadata
{
    public Category(LocalizedString name, LocalizedString description) : base(name, description)
    {
    }
}