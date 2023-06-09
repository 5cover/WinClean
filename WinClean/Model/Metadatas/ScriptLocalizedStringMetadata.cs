using System.Globalization;

namespace Scover.WinClean.Model.Metadatas;

/// <summary>Common properties for script metadata objects.</summary>
public abstract record ScriptLocalizedStringMetadata : IMetadata
{
    private readonly LocalizedString _description;

    private readonly LocalizedString _name;

    protected ScriptLocalizedStringMetadata(LocalizedString name, LocalizedString description) => (_name, _description) = (name, description);

    public string Description => _description[CultureInfo.CurrentUICulture];
    public string InvariantName => _name[CultureInfo.InvariantCulture];
    public string Name => _name[CultureInfo.CurrentUICulture];
    public string InvariantDescription => _description[CultureInfo.InvariantCulture];

    public int CompareTo(object? obj) => Name.CompareTo((obj as IMetadata)?.Name);
}