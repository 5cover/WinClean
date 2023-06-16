using Scover.WinClean.Model.Scripts;

namespace Scover.WinClean.Model.Metadatas;

/// <summary>What a script can be used as.</summary>
public sealed class Usage : Metadata
{
    private readonly IReadOnlyCollection<Capability> _capabilities;
    private readonly int _order;

    private Usage(int order, string resourceName, params Capability[] capabilities) : base(new ResourceTextProvider(Resources.Usages.ResourceManager, resourceName))
        => (_order, _capabilities) = (order, capabilities);

    public static Usage Actions { get; } = new(0, nameof(Actions), Capability.Execute);
    public static IReadOnlyCollection<Usage> Instances => Multiton<Usage, Usage>.Instances;
    public static Usage Settings { get; } = new(1, nameof(Settings), Capability.Enable, Capability.Disable);

    public static IEnumerable<Usage> GetUsages(Script script)
        => Instances.Where(usage => usage._capabilities.All(script.Code.Keys.Contains));

    public override int CompareTo(Metadata? other) => _order.CompareTo((other as Usage)?._order);
}