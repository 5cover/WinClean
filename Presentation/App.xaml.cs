global using System.IO;

global using static Humanizer.StringExtensions;

using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
public sealed partial class App
{
    private record Callbacks(Action WarnOnUpdate,
                             InvalidScriptDataCallback ReloadElseIgnore,
                             Action<Exception> WarnOnUnhandledException);

    private static readonly List<Script> _defaultScripts = new();
    private static CustomScriptCollection? _customScripts;
    private static Logger? _logger;

    /// <summary>Gets the concatenation of the default and custom scripts.</summary>
    public static IEnumerable<Script> AllScripts => App.DefaultScripts.Concat(App.CustomScripts);

    /// <summary>Gets the collection of custom scripts created by the user.</summary>
    public static CustomScriptCollection CustomScripts => _customScripts.AssertNotNull();

    /// <summary>
    /// Gets the application's default scripts, available after startup. They are immutable and deplyed at installation time.
    /// </summary>
    public static IReadOnlyCollection<Script> DefaultScripts => _defaultScripts;

    /// <summary>Gets the application's logger, available after startup.</summary>
    public static Logger Logger => _logger.AssertNotNull();

    private static void Initialize(Logger logger, Callbacks callbacks)
    {
        // 1. Add the unhandled exception handler
        Current.DispatcherUnhandledException += (_, args) => callbacks.WarnOnUnhandledException(args.Exception);

        // 2. Set the logger
        _logger = logger;

        // 3. Check for updates
        if (SourceControlClient.Instance.Value.LatestVersionName != AppInfo.Version)
        {
            callbacks.WarnOnUpdate();
        }

        // 4. Load default scripts
        Assembly assembly = Assembly.GetExecutingAssembly();
        IScriptSerializer s = new ScriptXmlSerializer();
        foreach (string scriptResName in assembly.GetManifestResourceNames().Where(name => name.StartsWith("Scover.WinClean.Scripts", StringComparison.InvariantCulture)))
        {
            _defaultScripts.Add(s.DeserializeDefault(assembly.GetManifestResourceStream(scriptResName).AssertNotNull()));
        }

        // 5. Load custom scripts.
        _customScripts = CustomScriptCollection.LoadScripts(AppDirectory.ScriptsDir, callbacks.ReloadElseIgnore);
    }

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
        CustomScripts.Save();
        AppInfo.Settings.Save();
        Logger.Log(Logs.Exiting);
    }

    private void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        if (e?.Args.Any() ?? false)
        {
            StartConsole(e.Args);
        }
        else
        {
            StartGui();
        }
    }
}