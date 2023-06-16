using System.Globalization;
using System.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class ResourceTextProvider : ITextProvider
{
    private readonly ResourceManager _resourceManager;

    public ResourceTextProvider(ResourceManager resourceManager, string resourceName) => (_resourceManager, ResourceName) = (resourceManager, resourceName);

    public string ResourceName { get; }

    public string GetDescription(CultureInfo culture) => _resourceManager.GetString(ResourceName + "Description").NotNull();

    public string GetName(CultureInfo culture) => _resourceManager.GetString(ResourceName).NotNull();
}