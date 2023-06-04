using System.Runtime.CompilerServices;

namespace Scover.WinClean.ViewModel.Logging;

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