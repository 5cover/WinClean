using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class Capability : Metadata
{
    private Capability(string resourceName) : base(new ResourceTextProvider(Capabilities.ResourceManager, resourceName))
        => ResourceName = resourceName;

    public static Capability Detect { get; } = new(nameof(Detect));
    public static Capability Disable { get; } = new(nameof(Disable));
    public static Capability Enable { get; } = new(nameof(Enable));
    public static Capability Execute { get; } = new(nameof(Execute));
    public static IReadOnlyCollection<Capability> Instances => Multiton<Capability, Capability>.Instances;
    public string ResourceName { get; }

    public static Capability? FromInteger(int number) => number switch
    {
        0 => Disable,
        1 => Enable,
        2 => Execute,
        3 => Detect,
        _ => null
    };

    /// <exception cref="InvalidOperationException">
    /// No capabaility exists with the specified resource name.
    /// </exception>
    public static Capability FromResourceName(string resourceName) => Multiton<Capability, Capability>.GetInstance(i => i.ResourceName == resourceName);

    public static Capability? FromResourceNameOrDefault(string resourceName) => Multiton<Capability, Capability>.GetInstanceOrDefault(i => i.ResourceName == resourceName);
}