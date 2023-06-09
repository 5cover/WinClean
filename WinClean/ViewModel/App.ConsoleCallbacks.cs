using Scover.WinClean.Model;
using Scover.WinClean.Resources;
using Scover.WinClean.ViewModel.Logging;

namespace Scover.WinClean.ViewModel;

public partial class App
{
    private static readonly Callbacks consoleCallbacks = new(
        NotifyUpdateAvailable: async () =>
        {
            var scc = await SourceControlClient.Instance;
            Console.WriteLine(ConsoleMode.Update.FormatWith(scc.LatestVersionName, scc.LatestVersionUrl));
        },
        ScriptLoadError: (e, path) =>
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
        WarnOnUnhandledException: e =>
        {
            Logs.UnhandledException.FormatWith(e).Log(LogLevel.Critical);
            return false;
        });
}