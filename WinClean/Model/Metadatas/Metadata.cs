using System.Diagnostics;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.Model.Metadatas;

[DebuggerDisplay($"{{{nameof(InvariantName)}}}")]
public abstract class Metadata : IComparable, IComparable<Metadata>
{
    private readonly ITextProvider _textProvider;

    protected Metadata(ITextProvider textProvider) => _textProvider = textProvider;

    /// <summary>Gets the description for <see cref="CurrentUICulture"/>.</summary>
    public string Description => _textProvider.GetDescription(CurrentUICulture);

    /// <summary>Gets the description for <see cref="InvariantCulture"/>.</summary>
    public string InvariantDescription => _textProvider.GetDescription(InvariantCulture);

    /// <summary>Gets the name for <see cref="InvariantCulture"/>.</summary>
    public string InvariantName => _textProvider.GetName(InvariantCulture);

    /// <summary>Gets the name for <see cref="CurrentUICulture"/>.</summary>
    public string Name => _textProvider.GetName(CurrentUICulture);

    public static bool operator !=(Metadata left, Metadata right) => !(left == right);

    public static bool operator <(Metadata left, Metadata right) => left.CompareTo(right) < 0;

    public static bool operator <=(Metadata left, Metadata right) => left.CompareTo(right) <= 0;

    public static bool operator ==(Metadata left, Metadata right) => left.Equals(right);

    public static bool operator >(Metadata left, Metadata right) => left.CompareTo(right) > 0;

    public static bool operator >=(Metadata left, Metadata right) => left.CompareTo(right) >= 0;

    public int CompareTo(object? obj) => CompareTo(obj as Metadata);

    public virtual int CompareTo(Metadata? other) => Name.CompareTo(other?.Name);

    public override bool Equals(object? obj) => obj is Metadata m && InvariantName == m.InvariantName && InvariantDescription == m.InvariantDescription;

    public override int GetHashCode() => HashCode.Combine(InvariantName, InvariantDescription);
}