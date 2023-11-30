using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Metadatas;

/// <summary>
/// Specifies which script selection state a capability belongs to.
/// </summary>
public enum CapabilityCorrespondingSelectionState
{
    /// <summary>
    /// The capability corresponds to a "Selected" selection state.
    /// </summary>
    Selected,

    /// <summary>
    /// The capability corresponds to an "Unselected" selection state.
    /// </summary>
    Unselected,

    /// <summary>
    /// The capability corresponds to an unspecified selection state.
    /// </summary>
    Unspecified,
}

public sealed class Capability : Metadata
{
    private Capability(string resourceName, CapabilityCorrespondingSelectionState correspondingSelectionState) : base(new ResourceTextProvider(Capabilities.ResourceManager, resourceName))
        => (CorrespondingSelectionState, ResourceName) = (correspondingSelectionState, resourceName);

    public static Capability Detect { get; } = new(nameof(Detect), CapabilityCorrespondingSelectionState.Unspecified);
    public static Capability Disable { get; } = new(nameof(Disable), CapabilityCorrespondingSelectionState.Unselected);
    public static Capability Enable { get; } = new(nameof(Enable), CapabilityCorrespondingSelectionState.Selected);
    public static Capability Execute { get; } = new(nameof(Execute), CapabilityCorrespondingSelectionState.Selected);
    public CapabilityCorrespondingSelectionState CorrespondingSelectionState { get; }
    public string ResourceName { get; }

    public static Capability? FromInteger(int number) => number switch
    {
        0 => Disable,
        1 => Enable,
        2 => Execute,
        3 => Detect,
        _ => null,
    };

    /// <exception cref="InvalidOperationException">
    /// No capability exists with the specified resource name.
    /// </exception>
    public static Capability FromResourceName(string resourceName) => Multiton<Capability, Capability>.GetInstance(i => i.ResourceName == resourceName);

    public static Capability? FromResourceNameOrDefault(string resourceName) => Multiton<Capability, Capability>.GetInstanceOrDefault(i => i.ResourceName == resourceName);
}