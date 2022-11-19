using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

using CommandLine;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

/// <summary>Handles the startup / shutdown strategy and holds data related to the Presentation layer.</summary>
public sealed partial class App : Application
{
    private static readonly ScriptXmlSerializer xmlSerializer = new();
    private static Logger? logger;

    static App()
        => ScriptCollections[ScriptType.Default].LoadAll(Assembly.GetExecutingAssembly().GetManifestResourceNames()
           .Where(name => name.StartsWith($"{nameof(Scover)}.{nameof(WinClean)}.{nameof(BusinessLogic.Scripts)}.", StringComparison.Ordinal)));

    /// <summary>Gets the logger, available after startup.</summary>
    public static Logger Logger => logger.AssertNotNull();

    public static IReadOnlyDictionary<ScriptType, ScriptCollection> ScriptCollections { get; } = new Dictionary<ScriptType, ScriptCollection>
    {
        [ScriptType.Custom] = new FileScriptCollection(AppDirectory.Scripts, xmlSerializer, ScriptType.Custom),
        [ScriptType.Default] = new ManifestResourceScriptCollection(xmlSerializer, ScriptType.Default)
    };

    private static void Initialize(Logger logger, Callbacks callbacks)
    {
        // 1. Add the unhandled exception handler
        Current.DispatcherUnhandledException += (_, args) => callbacks.WarnOnUnhandledException(args.Exception);

        // 2. Set the logger
        App.logger = logger;

        // 3. Check for updates
        if (SourceControlClient.Instance.Value.LatestVersionName != AppInfo.Version)
        {
            callbacks.WarnOnUpdate();
        }

        // 4. Load custom scripts.
        ScriptCollections[ScriptType.Custom].LoadAll(Directory.EnumerateFiles(AppDirectory.Scripts, '*' + AppInfo.Settings.ScriptFileExtension, SearchOption.AllDirectories),
                                                     callbacks.ReloadElseIgnoreInvalidCustomScript);
    }

    private record Callbacks(Action WarnOnUpdate,
                             InvalidScriptDataCallback ReloadElseIgnoreInvalidCustomScript,
                             Action<Exception> WarnOnUnhandledException);

    private static void StartConsole(string[] args)
    {
        // Get invariant (en-US) output.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(uint dwProcessId);
        _ = AttachConsole(unchecked((uint)-1));

        Initialize(new ConsoleLogger(), consoleCallbacks);

        _ = Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(options => Environment.ExitCode = options.Execute());

        Current.Shutdown();
    }

    private static void StartGui()
    {
        Initialize(new CsvLogger(), uiCallbacks);
        new MainWindow().Show();
    }

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Logger.Log(Logs.SavingScriptsAndSettings);

        foreach (var collection in ScriptCollections.Values.OfType<IMutableScriptCollection>())
        {
            collection.Save();
        }

        AppInfo.Settings.Save();
        AppInfo.PersistentSettings.Save();

        Logger.Log(Logs.Exiting);
    }

    private void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        if (e is not null && e.Args.Any())
        {
            StartConsole(e.Args);
        }
        else
        {
            StartGui();
        }
    }
}