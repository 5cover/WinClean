using Scover.WinClean.BusinessLogic;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

public partial class App
{
    private static readonly Callbacks consoleCallbacks = new(
        NotifyUpdateAvailable: async () =>
        {
            var scc = await SourceControlClient.Instance;
            WinClean.Resources.CommandLine.Update.FormatWith(scc.LatestVersionName, scc.LatestVersionUrl).Log(LogLevel.Info);
        },
        InvalidScriptData: (e, path) =>
        {
            // Log the error, but ignore invalid scripts.
            Logs.ScriptNotLoaded.FormatWith(path, e).Log(LogLevel.Error);
            return InvalidScriptDataAction.Ignore;
        },
        FSErrorReloadElseIgnore: (e) =>
        {
            Logs.ScriptNotLoaded.FormatWith(e.Element, e).Log(LogLevel.Error);
            return false;
        },
        WarnOnUnhandledException: e => Logs.UnhandledException.FormatWith(e).Log(LogLevel.Critical));
}