using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

public partial class App
{
    private static readonly Callbacks consoleCallbacks = new(
        (e, path) =>
        {
            // Log the error, but ignore invalid scripts.
            Logs.ScriptNotLoaded.FormatWith(path, e).Log(LogLevel.Error);
            return false;
        },
        (e, _, info) =>
        {
            Logs.ScriptNotLoaded.FormatWith(info.FullName, e).Log(LogLevel.Error);
            return false;
        },
        e => Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical));
}