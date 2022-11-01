using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

public partial class App
{
    private static readonly Callbacks consoleCallbacks = new(
        () =>
        {
            var sdd = SourceControlClient.Instance.Value;
            Logger.Log(WinClean.Resources.CommandLine.Update.FormatWith(sdd.LatestVersionName, sdd.LatestVersionUrl), LogLevel.Info);
        },
        (e, path) =>
        {
            // Log the error, but ignore invalid scripts.
            Logger.Log($"{Logs.InvalidScriptData.FormatWith(Path.GetFileName(path))}\n{e}", LogLevel.Error);
            return false;
        },
        e => Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical));
}