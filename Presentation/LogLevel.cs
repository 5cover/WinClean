namespace Scover.WinClean.Presentation;

/// <summary>Specifies a minimum log level.</summary>
public class LogLevel
{
    private LogLevel(string name, string? localizedName, int value)
    {
        Name = name;
        LocalizedName = localizedName;
        Value = value;
    }

    /// <summary>Unrecoverable errors. The application can't continue.</summary>
    public static LogLevel Critical { get; } = new(nameof(Critical), Resources.LogLevel.Critical, 4);

    /// <summary>Error-level entries minimum.</summary>
    public static LogLevel Error { get; } = new(nameof(Error), Resources.LogLevel.Error, 3);

    /// <summary>Informational entries minimum.</summary>
    public static LogLevel Info { get; } = new(nameof(Info), Resources.LogLevel.Info, 1);

    public static IEnumerable<LogLevel> Values => new[]
    {
        Verbose,
        Info,
        Warning,
        Error,
        Critical
    };

    /// <summary>All entries are logged.</summary>
    public static LogLevel Verbose { get; } = new(nameof(Verbose), Resources.LogLevel.Verbose, 0);

    /// <summary>Warning-level entries minimum.</summary>
    public static LogLevel Warning { get; } = new(nameof(Warning), Resources.LogLevel.Warning, 2);

    public string? LocalizedName { get; }
    public string Name { get; }
    public int Value { get; }
}