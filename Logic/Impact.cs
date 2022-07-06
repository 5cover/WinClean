using Scover.WinClean.Operational;

namespace Scover.WinClean.Logic;

/// <summary>Effect of running a script.</summary>
public class Impact
{
    private readonly string _name;

    private Impact(string name, string? localizedName)
    {
        LocalizedName = localizedName;
        _name = name;
    }

    /// <summary>System praticality.</summary>
    public static Impact Ergonomics { get; } = new(nameof(Ergonomics), Resources.ImpactEffect.Ergonomics);

    /// <summary>Free storage space.</summary>
    public static Impact FreeStorageSpace { get; } = new(nameof(FreeStorageSpace), Resources.ImpactEffect.FreeStorageSpace);

    /// <summary>Idle system memory usage.</summary>
    public static Impact MemoryUsage { get; } = new(nameof(MemoryUsage), Resources.ImpactEffect.MemoryUsage);

    /// <summary>Idle system network usage.</summary>
    public static Impact NetworkUsage { get; } = new(nameof(NetworkUsage), Resources.ImpactEffect.NetworkUsage);

    /// <summary>System privacy invasion and spying.</summary>
    public static Impact Privacy { get; } = new(nameof(Privacy), Resources.ImpactEffect.Privacy);

    /// <summary>System shutdown time.</summary>
    public static Impact ShutdownTime { get; } = new(nameof(ShutdownTime), Resources.ImpactEffect.ShutdownTime);

    /// <summary>System overall stability.</summary>
    public static Impact Stability { get; } = new(nameof(Stability), Resources.ImpactEffect.Stability);

    /// <summary>System startup time.</summary>
    public static Impact StartupTime { get; } = new(nameof(StartupTime), Resources.ImpactEffect.StartupTime);

    /// <summary>Storage read-write speed.</summary>
    public static Impact StorageSpeed { get; } = new(nameof(StorageSpeed), Resources.ImpactEffect.StorageSpeed);

    /// <summary>Gets all the values.</summary>
    public static IEnumerable<Impact> Values => new[]
    {
        Ergonomics,
        FreeStorageSpace,
        MemoryUsage,
        NetworkUsage,
        Privacy,
        Stability,
        ShutdownTime,
        StartupTime,
        StorageSpeed,
        Visuals
    };

    /// <summary>System visuals.</summary>
    public static Impact Visuals { get; } = new(nameof(Visuals), Resources.ImpactEffect.Visuals);

    public string? LocalizedName { get; }

    /// <summary>Gets the <see cref="Impact"/> matching the specified name.</summary>
    /// <exception cref="ArgumentException"><paramref name="name"/> does not match to any <see cref="Impact"/> name.</exception>
    public static Impact ParseName(string name)
        => Values.SingleOrDefault(validValue => validValue._name == name)
           ?? throw new ArgumentException(Resources.DevException.InvalidTypeProp.FormatWithInvariant(nameof(Impact), nameof(_name)), nameof(name));

    public override string ToString() => _name;
}