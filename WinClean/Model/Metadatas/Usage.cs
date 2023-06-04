using Scover.WinClean.Model.Scripts;

namespace Scover.WinClean.Model.Metadatas;

/// <summary>
/// What a script can be used as.
/// </summary>
public sealed class Usage : ScriptResourceMetadata
{
    private readonly IReadOnlyCollection<Capability> _capabilities;

    private Usage(string resourceName, params Capability[] capabilities) : base(Resources.Usages.ResourceManager, resourceName, resourceName + "Description")
        => _capabilities = capabilities;

    public static Usage Actions { get; } = new(nameof(Actions), Capability.Execute);
    public static Usage Settings { get; } = new(nameof(Settings), Capability.Enable, Capability.Disable);
    public static Usage Other { get; } = new(nameof(Other));
    public static IReadOnlyCollection<Usage> Instances => Multiton<Usage, Usage>.Instances;

    public static Usage Get(Script script)
        => Instances.Where(usage => usage._capabilities.All(script.Code.Keys.Contains)).MaxBy(u => u._capabilities.Count).AssertNotNull();
}
