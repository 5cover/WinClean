namespace Scover.WinClean.Model.Metadatas;

public abstract class OrderedMetadata : Metadata, IComparable, IComparable<OrderedMetadata>
{
    private readonly int _order;
    protected OrderedMetadata(ITextProvider textProvider, int order) : base(textProvider) => _order = order;

    public int CompareTo(object? obj) => CompareTo(obj as OrderedMetadata);
    public int CompareTo(OrderedMetadata? other) => _order.CompareTo(other?._order);

    public override bool Equals(object? obj) => _order.Equals((obj as OrderedMetadata)?._order);

    public override int GetHashCode() => _order.GetHashCode();

    public static bool operator ==(OrderedMetadata left, OrderedMetadata right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(OrderedMetadata left, OrderedMetadata right) => !(left == right);

    public static bool operator <(OrderedMetadata left, OrderedMetadata right) => left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator <=(OrderedMetadata left, OrderedMetadata right) => left is null || left.CompareTo(right) <= 0;

    public static bool operator >(OrderedMetadata left, OrderedMetadata right) => left is not null && left.CompareTo(right) > 0;

    public static bool operator >=(OrderedMetadata left, OrderedMetadata right) => left is null ? right is null : left.CompareTo(right) >= 0;
}