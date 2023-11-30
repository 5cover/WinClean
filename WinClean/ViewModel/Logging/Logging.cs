using System.Runtime.CompilerServices;

namespace Scover.WinClean.ViewModel.Logging;

public enum LoggingType
{
    Console,
    Csv,
}

public static partial class Logging
{
    public static Logger Logger { get; private set; } = new MockLogger();

    public static void InitializeLogger(LoggingType desiredLoggingType) => Logger = desiredLoggingType switch
    {
        LoggingType.Console => new ConsoleLogger(),
        LoggingType.Csv => new CsvLogger(),
        _ => throw desiredLoggingType.NewInvalidEnumArgumentException(nameof(desiredLoggingType)),
    };

    /// <inheritdoc cref="Logger.Log(string, LogLevel, string, int, string)"/>
    public static void Log(this string message,
        LogLevel lvl = LogLevel.Verbose,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callLine = 0,
        [CallerFilePath] string callFile = "")
        => Logger.Log(message, lvl, caller, callLine, callFile);
}