using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;

using CommandLine;

using Humanizer;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

/// <summary>Handles the startup / shutdown strategy and holds data related to the Presentation layer.</summary>
public sealed partial class App
{
    private static readonly ScriptXmlSerializer xmlSerializer = new();
    private static Logger? logger;

    /// <summary>Gets the logger of the application.</summary>
    /// <remarks>Available after startup.</remarks>
    public static Logger Logger => logger.AssertNotNull();

    /// <summary>Gets the collection of scripts that was initially loaded when the application started.</summary>
    /// <remarks>Available after startup.</remarks>
    public static IEnumerable<Script> Scripts => scriptCollections.Values.SelectMany(c => c);

    private static Dictionary<ScriptType, ScriptCollection> scriptCollections { get; } = new();

    /// <summary>Commits all changes to the scripts.</summary>
    /// <param name="scripts">The modified script collection.</param>
    public static void SaveScripts(IReadOnlyCollection<Script> scripts)
    {
        // Remove scripts first to avoid duplicates.
        foreach (var removedScript in Scripts.Except(scripts))
        {
            if (scriptCollections[removedScript.Type] is IMutableScriptCollection collection)
            {
                collection.Remove(removedScript);
            }
        }

        foreach (var addedScript in scripts.Except(Scripts))
        {
            if (scriptCollections[addedScript.Type] is IMutableScriptCollection collection)
            {
                collection.Add(addedScript);
            }
        }

        foreach (var collection in scriptCollections.Values.OfType<IMutableScriptCollection>())
        {
            collection.Save();
        }

        Logs.ScriptsSaved.Log();
    }

    private static void Initialize(Logger loggerToUse, Callbacks callbacks)
    {
        // 1. Add the unhandled exception handler
        Current.DispatcherUnhandledException += (_, args) => callbacks.WarnOnUnhandledException(args.Exception);

        // 2. Set the logger
        logger = loggerToUse;

        // 3. Check for updates
        if (SourceControlClient.Instance.Value.LatestVersionName != AppInfo.Version)
        {
            callbacks.WarnOnUpdate();
        }

        // 4. Load scripts.
        scriptCollections[ScriptType.Custom] = new FileScriptCollection(AppDirectory.Scripts, AppInfo.Settings.ScriptFileExtension,
            callbacks.ReloadElseIgnoreInvalidCustomScript, callbacks.ReloadElseIgnoreFSErrorAcessingCustomScript, xmlSerializer, ScriptType.Custom);
        scriptCollections[ScriptType.Default] = new ManifestResourceScriptCollection(
            $"{nameof(Scover)}.{nameof(WinClean)}.{nameof(BusinessLogic.Scripts)}", xmlSerializer, ScriptType.Default);
    }

    private sealed record Callbacks(Action WarnOnUpdate,
                             InvalidScriptDataCallback ReloadElseIgnoreInvalidCustomScript,
                             FSErrorCallback ReloadElseIgnoreFSErrorAcessingCustomScript,
                             Action<Exception> WarnOnUnhandledException);

    private static void StartConsole(IEnumerable<string> args)
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
        AppInfo.Settings.Save();
        AppInfo.PersistentSettings.Save();
        Logs.SettingsSaved.Log();
        Logs.Exiting.Log();
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