using System.Globalization;
using System.Resources;

namespace Scover.WinClean.Model;

public abstract class ResourceNamedObject
{
    protected ResourceManager ResourceManager { get; }

    protected ResourceNamedObject(ResourceManager resourceManager, string resourceName) => (ResourceManager, ResourceName) = (resourceManager, resourceName);

    public string InvariantName => ResourceManager.GetString(ResourceName, CultureInfo.InvariantCulture).AssertNotNull();
    public string Name => ResourceManager.GetString(ResourceName).AssertNotNull();
    public string ResourceName { get; }

    public override string ToString() => InvariantName;
}
