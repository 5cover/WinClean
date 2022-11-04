using System.Globalization;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>Common properties for script metadata objects.</summary>
public abstract class ScriptMetadata
{
    private readonly LocalizedString _description;

    private readonly LocalizedString _name;

    /// <summary>Initializes a new <see cref="ScriptMetadata"/> object.</summary>
    /// <param name="name">The localized name.</param>
    /// <param name="description">The localized description.</param>
    protected ScriptMetadata(LocalizedString name, LocalizedString description) => (_name, _description) = (name, description);

    /// <summary>Gets the description for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    public string Description => _description.Get(CultureInfo.CurrentUICulture);

    /// <summary>Gets the name for <see cref="CultureInfo.InvariantCulture"/>.</summary>
    public string InvariantName => _name.Get(CultureInfo.InvariantCulture);

    /// <summary>Gets the name for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    public string Name => _name.Get(CultureInfo.CurrentUICulture);
}