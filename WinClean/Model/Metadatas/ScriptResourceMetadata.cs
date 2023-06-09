using System.Globalization;
using System.Resources;

namespace Scover.WinClean.Model.Metadatas;

public abstract class ScriptResourceMetadata : ResourceNamedObject, IMetadata
{
    protected ScriptResourceMetadata(ResourceManager resourceManager, string resourceName, string descriptionResourceName) : base(resourceManager, resourceName)
        => DescriptionResourceName = descriptionResourceName;

    public string Description => ResourceManager.GetString(DescriptionResourceName).NotNull();
    public string DescriptionResourceName { get; }
    public string InvariantDescription => ResourceManager.GetString(DescriptionResourceName, CultureInfo.InvariantCulture).NotNull();

    public int CompareTo(object? obj) => Name.CompareTo((obj as IMetadata)?.Name);
}