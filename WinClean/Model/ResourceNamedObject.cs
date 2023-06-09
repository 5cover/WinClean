using System.Globalization;
using System.Resources;

namespace Scover.WinClean.Model;

public abstract class ResourceNamedObject
{
    protected ResourceNamedObject(ResourceManager resourceManager, string resourceName) => (ResourceManager, ResourceName) = (resourceManager, resourceName);

    public string InvariantName => ResourceManager.GetString(ResourceName, CultureInfo.InvariantCulture).NotNull();
    public string Name => ResourceManager.GetString(ResourceName).NotNull();
    public string ResourceName { get; }
    protected ResourceManager ResourceManager { get; }

    public override string ToString() => InvariantName;
}