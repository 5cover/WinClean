using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;

using CommandLine;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Properties;
using Scover.WinClean.Resources;

using Vanara.PInvoke;

namespace Scover.WinClean.Presentation;

/// <summary>
/// Handles the startup / shutdown strategy and holds data related to the Presentation layer.
/// </summary>
public sealed partial class App
{
    private const string ScriptExecutionTimesFormatString = "c";
    private static readonly string scriptExecutionTimesSeparator = Environment.NewLine;

    static App() => ScriptExecutionTimes = Settings.ScriptExecutionTimes?.ParseMockDictionary(scriptExecutionTimesSeparator).ToDictionary(kv => kv.Key,
                kv => TimeSpan.ParseExact(kv.Value, ScriptExecutionTimesFormatString, CultureInfo.InvariantCulture)) ?? new Dictionary<string, TimeSpan>();

    public static Logger Logger { get; private set; } = new MockLogger();

    /// <summary>
    /// Gets the script execution times dictionary, keyed by <see cref="IScript.InvariantName"/>.
    /// </summary>
    public static IDictionary<string, TimeSpan> ScriptExecutionTimes { get; }

    public static ScriptStorage Scripts { get; } = new();
    public static Settings Settings => Settings.Default;

    private static async Task Initialize(Callbacks callbacks)
    {
        Current.DispatcherUnhandledException += (_, args) => callbacks.WarnOnUnhandledException(args.Exception);
        Scripts.Load(callbacks.InvalidScriptData, callbacks.FSErrorReloadElseIgnore);
        var scc = await SourceControlClient.Instance;
        if (!Settings.ShowUpdateDialog || scc.LatestVersionName == AppInfo.Version)
        {
            await callbacks.NotifyUpdateAvailable();
        }
    }

    private static async Task StartConsole(IEnumerable<string> args)
    {
        // Get invariant (en-US) output.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        // Attach console to current process
        _ = Win32Error.ThrowLastErrorIfFalse(Kernel32.AttachConsole(Kernel32.ATTACH_PARENT_PROCESS));

        Logger = new ConsoleLogger();
        await Initialize(consoleCallbacks);

        _ = Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(options => Environment.ExitCode = options.Execute(Scripts));

        Current.Shutdown();
    }

    private static async Task StartGui()
    {
        Logger = new CsvLogger();
        await Initialize(uiCallbacks);
        new MainWindow(Scripts).Show();
    }

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Settings.ScriptExecutionTimes = ScriptExecutionTimes.ToMockStringDic(scriptExecutionTimesSeparator, valueFormatter: value => value.ToString(ScriptExecutionTimesFormatString));
        Settings.Save();
        Logs.SettingsSaved.Log();
        Logs.Exiting.Log();
    }

    private async void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        if (e is not null && e.Args.Any())
        {
            await StartConsole(e.Args);
        }
        else
        {
            await StartGui();
        }
    }

    private sealed record Callbacks(
        Func<Task> NotifyUpdateAvailable,
        InvalidScriptDataCallback InvalidScriptData,
        FSErrorCallback FSErrorReloadElseIgnore,
        Action<Exception> WarnOnUnhandledException);
}