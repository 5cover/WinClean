using System.Runtime.CompilerServices;

namespace Scover.WinClean.Presentation.Logging;

public abstract class Logger
{
    /// <summary>Gets or sets the minimal log level for a logging operation to occur.</summary>
    /// <remarks>Default value is <see cref="LogLevel.Verbose"/>.</remarks>
    public LogLevel MinLevel { get; set; } = LogLevel.Verbose;

    /// <summary>Clears the logs.</summary>
    public abstract void ClearLogs();

    /// <summary>Logs a string.</summary>
    /// <param name="message">The string to log.</param>
    /// <param name="lvl">The level of the log entry.</param>
    /// <param name="caller"><see cref="CallerMemberNameAttribute"/> - Don't specify</param>
    /// <param name="callLine"><see cref="CallerLineNumberAttribute"/> - Don't specify</param>
    /// <param name="callFile"><see cref="CallerFilePathAttribute"/> - Don't specify</param>
    public void Log(string message,
             LogLevel lvl = LogLevel.Verbose,
             [CallerMemberName] string caller = "",
             [CallerLineNumber] int callLine = 0,
             [CallerFilePath] string callFile = "")
    {
        if (lvl >= MinLevel)
        {
            Log(new(lvl.GetName(),
                    DateTime.Now,
                    message,
                    caller,
                    callLine,
                    // Only keep the filename of the source file to avoid showing personal information in file paths.
                    Path.GetFileName(callFile)));
        }
    }

    protected abstract void Log(LogEntry entry);

    ///<remarks>Topmost = leftmost.</remarks>
    protected sealed record LogEntry(string Level,
                            DateTime Date,
                            string Message,
                            string Caller,
                            int CallLine,
                            string CallFile);
}

public static class LoggerExtensions
{
    /// <inheritdoc cref="Logger.Log(string, LogLevel, string, int, string)"/>
    public static void Log(this string message,
        LogLevel lvl = LogLevel.Verbose,
        [CallerMemberName] string caller = "",
        [CallerLineNumber] int callLine = 0,
        [CallerFilePath] string callFile = "")
        => App.Logger.Log(message, lvl, caller, callLine, callFile);
}