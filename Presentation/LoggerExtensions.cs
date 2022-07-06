using System.Runtime.CompilerServices;

namespace Scover.WinClean.Presentation;

public static class LoggerExtensions
{
    /// <inheritdoc cref="Logger.Log(string, LogLevel, string, int, string)"/>
    public static void Log(this string str, LogLevel? lvl = null,
                           [CallerMemberName] string caller = "(error)",
                           [CallerLineNumber] int callLine = 0,
                           [CallerFilePath] string callFile = "(error)")
        => Logger.Instance.Log(str, lvl, caller, callLine, callFile);

    public static void SetAsHappening(this string str) => Logger.Instance.Happening = str;
}