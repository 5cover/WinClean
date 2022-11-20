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
            WinClean.Resources.CommandLine.Update.FormatWith(sdd.LatestVersionName, sdd.LatestVersionUrl).Log(LogLevel.Info);
        },
        (e, path) =>
        {
            // Log the error, but ignore invalid scripts.
            Logs.InvalidScriptData.FormatWith(path, e).Log(LogLevel.Error);
            return false;
        },
        (e, _, info) =>
        {
            Logs.FSErrorLoadingCustomScript.FormatWith(info.FullName, e).Log(LogLevel.Error);
            return false;
        },
        e => Logger.Log(Logs.UnhandledException.FormatWith(e), LogLevel.Critical));
}