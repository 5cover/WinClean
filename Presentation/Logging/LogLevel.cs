using System.Globalization;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation.Logging;

public enum LogLevel
{
    Verbose = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

public static class LogLevelExtensions
{
    public static string GetName(this LogLevel logLevel)
        => Resources.LogLevels.ResourceManager.GetString(Enum.GetName(logLevel).AssertNotNull(), CultureInfo.CurrentUICulture).AssertNotNull();
}