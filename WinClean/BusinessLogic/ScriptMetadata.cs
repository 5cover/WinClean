using System.Globalization;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Common properties for script metadata objects.</summary>
public abstract record ScriptMetadata
{
    private readonly LocalizedString _description;

    private readonly LocalizedString _name;

    protected ScriptMetadata(LocalizedString name, LocalizedString description) => (_name, _description) = (name, description);

    /// <summary>Gets the description for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    public string Description => _description.Get(CultureInfo.CurrentUICulture);

    /// <summary>Gets the name for <see cref="CultureInfo.InvariantCulture"/>.</summary>
    public string InvariantName => _name.Get(CultureInfo.InvariantCulture);

    /// <summary>Gets the name for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    public string Name => _name.Get(CultureInfo.CurrentUICulture);
}