using System.Collections.Specialized;
using System.Globalization;
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

/// <summary>Handles the startup / shutdown strategy and holds data related to the Presentation layer.</summary>
public sealed partial class App
{
    private const string ScriptsResourceNamespace = $"{nameof(Scover)}.{nameof(WinClean)}.{nameof(BusinessLogic.Scripts)}";
    private const string MetadataContentFilesNamespace = $"{nameof(Scover)}.{nameof(WinClean)}";
    private const string ScriptExecutionTimesFormatString = "c";

    private static Logger? logger;

    public static Settings Settings => Settings.Default;

    static App()
    {
        Scover.Dialogs.Dialog.UseActivationContext = false;
        Settings.Default[nameof(Settings.ScriptExecutionTimes)] ??= new StringCollection();
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

    /// <summary>Gets the script execution times dictionary, keyed by <see cref="Script.InvariantName"/>.</summary>
    public static IDictionary<string, TimeSpan> ScriptExecutionTimes { get; }

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
        IScriptSerializer serializer = new ScriptXmlSerializer(ScriptMetadata);

        // 1. Add the unhandled exception handler
        Current.DispatcherUnhandledException += (_, args) => callbacks.WarnOnUnhandledException(args.Exception);

        // 2. Set the logger
        logger = loggerToUse;

        // 3. Check for updates
        if (SourceControlClient.Instance.LatestVersionName != AppMetadata.Version)
        {
            callbacks.WarnOnUpdate();
        }

        // 4. Load scripts.
        scriptCollections[ScriptType.Custom] = new FileScriptCollection(AppDirectory.Scripts, Settings.ScriptFileExtension,
            callbacks.ReloadElseIgnoreInvalidCustomScript, callbacks.ReloadElseIgnoreFSErrorAcessingCustomScript, serializer, ScriptType.Custom);
        scriptCollections[ScriptType.Default] = new ManifestResourceScriptCollection(
            $"{nameof(Scover)}.{nameof(WinClean)}.{nameof(BusinessLogic.Scripts)}", serializer, ScriptType.Default);
    }

    private sealed record Callbacks(
        Action WarnOnUpdate,
        InvalidScriptDataCallback ReloadElseIgnoreInvalidCustomScript,
        FSErrorCallback ReloadElseIgnoreFSErrorAcessingCustomScript,
        Action<Exception> WarnOnUnhandledException);

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

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Settings.ScriptExecutionTimes.SetContent(ScriptExecutionTimes.Select(kv => (kv.Key, kv.Value.ToString(ScriptExecutionTimesFormatString))));
        Settings.Save();
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

    public static TypedEnumerablesDictionary ScriptMetadata { get; }

    private static Stream ReadContentFile(string filename)
#if PORTABLE
        => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{MetadataContentFilesNamespace}.{filename}").AssertNotNull();
#else
        => File.OpenRead(Path.Join(AppDomain.CurrentDomain.BaseDirectory, filename));

#endif
}