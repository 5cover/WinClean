using System.Globalization;

using Scover.WinClean.BusinessLogic;

namespace Scover.WinClean.Presentation;

/// <summary>Specifies a minimum log level.</summary>
public class LogLevel
{
    private readonly LocalizedString _name = new();

    private LogLevel(string name, string localizedName, int value)
    {
        _name.Set(CultureInfo.InvariantCulture, name);
        _name.Set(CultureInfo.CurrentUICulture, localizedName);

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

    public string InvariantName => _name.Get(CultureInfo.InvariantCulture);
    public string Name => _name.Get(CultureInfo.CurrentUICulture);
    public int Value { get; }

    public override string ToString() => _name.Get(CultureInfo.InvariantCulture);
}