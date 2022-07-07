using System.Runtime.CompilerServices;

namespace Scover.WinClean.Presentation;

public static class LogsDirectoryExtensions
{
    /// <inheritdoc cref="LogsDirectory.Log(string, LogLevel, string, int, string)"/>
    public static void Log(this string str, LogLevel? lvl = null,
                           [CallerMemberName] string caller = "(error)",
                           [CallerLineNumber] int callLine = 0,
                           [CallerFilePath] string callFile = "(error)")
        => LogsDirectory.Instance.Log(str, lvl, caller, callLine, callFile);

    public static void SetAsHappening(this string str) => LogsDirectory.Instance.Happening = str;
}