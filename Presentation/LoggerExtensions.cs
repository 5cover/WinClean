namespace Scover.WinClean.Presentation;

public static class LoggerExtensions
{
    /// <inheritdoc cref="Logger.Log(string, LogLevel, string, int, string)"/>
    public static void Log(this string str, LogLevel? lvl = null) => Logger.Instance.Log(str, lvl);

    public static void SetAsHappening(this string str) => Logger.Instance.Happening = str;
}