namespace Scover.WinClean.DataAccess;

public enum EventType
{
    None,

    /// <summary>
    /// A system change has begun. A subsequent nested call does not create a new restore point.
    /// <para>Subsequent calls must use <see cref="EndNestedSystemChange"/>, not <see cref="EndSystemChange"/>.</para>
    /// </summary>
    BeginNestedSystemChange = 0x66,

    /// <summary>A system change has begun.</summary>
    BeginSystemChange = 0x64,

    /// <summary>A system change has ended.</summary>
    EndNestedSystemChange = 0x67,

    /// <summary>A system change has ended.</summary>
    EndSystemChange = 0x65
}
