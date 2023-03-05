using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using System.Windows;

using CommandLine;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
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
    private const string MetadataContentFilesNamespace = $"{nameof(Scover)}.{nameof(WinClean)}";
    private const string ScriptExecutionTimesFormatString = "c";
    private const string DefaultScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(WinClean)}.{nameof(BusinessLogic.Scripts)}";

    private static readonly List<IDisposable> applicationLifetimeObjects = new();
    private static Logger? logger;

    static App()
    {
        Settings.Default[nameof(Settings.ScriptExecutionTimes)] ??= new System.Collections.Specialized.StringCollection();

        ScriptExecutionTimes = Settings.ScriptExecutionTimes.ParseKeysAndValues().ToDictionary(kv => kv.key.AssertNotNull(),
            kv => TimeSpan.ParseExact(kv.value.AssertNotNull(), ScriptExecutionTimesFormatString, CultureInfo.InvariantCulture));

        IScriptMetadataDeserializer d = new ScriptMetadataXmlDeserializer();
        // Explicitely enumerate the metadata lists.
        ScriptMetadata = new()
        {
            d.GetCategories(ReadContentFile("Categories.xml")).ToList(),
            d.GetHosts(ReadContentFile("Hosts.xml")).ToList(),
            d.GetImpacts(ReadContentFile("Impacts.xml")).ToList(),
            d.GetRecommendationLevels(ReadContentFile("RecommendationLevels.xml")).ToList()
        };
    }

    /// <summary>Gets the logger of the application.</summary>
    /// <remarks>Available after startup.</remarks>
    public static Logger Logger => logger.AssertNotNull();

    /// <summary>
    /// Gets the script execution times dictionary, keyed by <see cref="Script.InvariantName"/>.
    /// </summary>
    public static IDictionary<string, TimeSpan> ScriptExecutionTimes { get; }

    public static TypedEnumerablesDictionary ScriptMetadata { get; }

    /// <summary>
    /// Gets the collection of scripts that was initially loaded when the application started.
    /// </summary>
    /// <remarks>Available after startup.</remarks>
    public static IEnumerable<Script> Scripts => scriptCollections.Values.SelectMany(c => c);

    public static Settings Settings => Settings.Default;
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
                _ = collection.Remove(removedScript);
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

    /// <summary>
    /// Keeps a reference to one or more disposable objects and schedules a call to <see
    /// cref="IDisposable.Dispose"/> on it on application exit.
    /// </summary>
    public static void RegisterForAppLifetime(params IDisposable[] disposables) => applicationLifetimeObjects.AddRange(disposables);

    private static void Initialize(Logger loggerToUse, Callbacks callbacks)
    {
        IScriptSerializer serializer = new ScriptXmlSerializer(ScriptMetadata);

        // 1. Add the unhandled exception handler
        Current.DispatcherUnhandledException += (_, args) => callbacks.WarnOnUnhandledException(args.Exception);

        // 2. Set the logger
        logger = loggerToUse;

        // 3. Load scripts.
        scriptCollections[ScriptType.Custom] = new FileScriptCollection(AppDirectory.Scripts, Settings.ScriptFileExtension,
            callbacks.ReloadElseIgnoreInvalidCustomScript, callbacks.ReloadElseIgnoreFSErrorAcessingCustomScript, serializer, ScriptType.Custom);
        scriptCollections[ScriptType.Default] = new ManifestResourceScriptCollection(DefaultScriptsResourceNamespace, serializer, ScriptType.Default);
    }

    private static void EnsureAdminPrivileges()
    {
        if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            return;
        }
        ProcessStartInfo processInfo = new(Environment.ProcessPath.AssertNotNull())
        {
            UseShellExecute = true,
            Verb = "runas"
        };
        try
        {
            _ = Process.Start(processInfo);
        }
        catch (Win32Exception)
        {
            // User clicked "No" in the UAC prompt
        }
        Current.Shutdown();
    }

    private static Stream ReadContentFile(string filename)
        => System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"{MetadataContentFilesNamespace}.{filename}").AssertNotNull();

    private static void StartConsole(IEnumerable<string> args)
    {
        // Get invariant (en-US) output.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        // Attach console to current process
        _ = Kernel32.AttachConsole(Kernel32.ATTACH_PARENT_PROCESS);

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

    private sealed record Callbacks(
        InvalidScriptDataCallback ReloadElseIgnoreInvalidCustomScript,
        FSErrorCallback ReloadElseIgnoreFSErrorAcessingCustomScript,
        Action<Exception> WarnOnUnhandledException);

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Settings.ScriptExecutionTimes.SetContent(ScriptExecutionTimes.ToDictionary(kv => kv.Key, kv => kv.Value.ToString(ScriptExecutionTimesFormatString)));
        Settings.Save();
        Logs.SettingsSaved.Log();
        Logs.Exiting.Log();
        foreach (var disposable in applicationLifetimeObjects)
        {
            disposable.Dispose();
        }
    }

    private void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        // ClickOnce doesn't support requireAdministrator in manifest, this is a workaround.
        EnsureAdminPrivileges();

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