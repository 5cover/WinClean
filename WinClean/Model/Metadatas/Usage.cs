using Scover.WinClean.Model.Scripts;

namespace Scover.WinClean.Model.Metadatas;

/// <summary>What a script can be used as.</summary>
public sealed class Usage : OrderedMetadata
{
    private readonly IReadOnlyCollection<Capability> _capabilities;

    private Usage(string resourceName, int order, params Capability[] capabilities) : base(new ResourceTextProvider(Resources.Usages.ResourceManager, resourceName), order)
        => _capabilities = capabilities;

    public static Usage Actions { get; } = new(nameof(Actions), 0, Capability.Execute);
    public static IReadOnlyCollection<Usage> Instances => Multiton<Usage, Usage>.Instances;
    public static Usage Settings { get; } = new(nameof(Settings), 1, Capability.Enable, Capability.Disable);

    public static IEnumerable<Usage> GetUsages(Script script)
        => Instances.Where(usage => usage._capabilities.All(script.Actions.Keys.Contains));
}