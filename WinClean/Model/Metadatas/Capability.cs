using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

public sealed class Capability : ScriptResourceMetadata
{
    private Capability(string resourceName) : base(Capabilities.ResourceManager, resourceName, resourceName + "Description")
    { }

    public static IReadOnlyCollection<Capability> Instances => Multiton<Capability, Capability>.Instances;
    public static Capability Detect { get; } = new(nameof(Detect));

    public static Capability Execute { get; } = new(nameof(Execute));

    public static Capability Disable { get; } = new(nameof(Disable));

    public static Capability Enable { get; } = new(nameof(Enable));

    public static Capability FromResourceName(string resourceName) => Multiton<Capability, Capability>.GetInstance(i => i.ResourceName == resourceName);

    public static Capability? FromInteger(int number) => number switch
    {
        0 => Disable,
        1 => Enable,
        2 => Execute,
        3 => Detect,
        _ => null
    };
}