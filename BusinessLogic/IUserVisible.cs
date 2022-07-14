namespace Scover.WinClean.BusinessLogic;

/// <summary>Represents an element that the user will be able to see and interact with.</summary>
public interface IUserVisible
{
    /// <summary>Gets the description of this instance.</summary>
    /// <value>A free-form description for this instance.</value>
    string Description { get; }

    /// <summary>Gets the invariant name of this instance.</summary>
    /// <value>A culture-independent, human-readable name for this instance that can be used to identify the value programatically.</value>
    string InvariantName { get; }

    /// <summary>Gets the name of this instance.</summary>
    /// <value>An user friendly name for this instance.</value>
    string Name { get; }
}