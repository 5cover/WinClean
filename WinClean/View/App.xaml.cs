using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

using CommandLine;

using Scover.WinClean.Model;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.View.Windows;
using Scover.WinClean.ViewModel.Logging;
using Scover.WinClean.ViewModel.Windows;

using Vanara.PInvoke;

namespace Scover.WinClean.View;

/// <summary>Handles the startup / shutdown strategy and holds data related to the View layer.</summary>
public sealed partial class App
{
    public static App CurrentApp => (App)Current;
    public Logger Logger { get; private set; } = new MockLogger();

    private static async Task CheckForUpdates(Callbacks callbacks)
    {
        if (ServiceProvider.Get<ISettings>().ShowUpdateDialog)
        {
            var latestVersion = (await SourceControlClient.Instance).LatestVersionName;
            var currentVersion = ServiceProvider.Get<IApplicationInfo>().Version;
            if (latestVersion != currentVersion)
            {
                Logs.UpdateAvailable.FormatWith(latestVersion, currentVersion).Log(LogLevel.Info);
                callbacks.NotifyUpdateAvailable(latestVersion);
            }
        }
    }

    private static Task LoadScripts(Callbacks callbacks) => ServiceProvider.Get<IScriptStorage>().LoadAsync(callbacks.ScriptLoadError, callbacks.FSErrorReloadElseIgnore);

    private void ApplicationExit(object? sender, ExitEventArgs e)
    {
        ServiceProvider.Get<ISettings>().Save();
        Logs.SettingsSaved.Log();
        Logs.Exiting.FormatWith(e.ApplicationExitCode).Log();
    }

    private async void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        if (e?.Args.Any() ?? false)
        {
            await StartConsole(e.Args);
        }
        else
        {
            await StartGui();
        }
    }

    private void Initialize(Callbacks callbacks)
    {
        // WPF trick thant makes text rendering slightly better on desktop platforms
        TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(TextFormattingMode.Display,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        var viewFactory = ServiceProvider.Get<IViewFactory>();
        viewFactory.Register<AboutViewModel, AboutWindow>();
        viewFactory.Register<SettingsViewModel, SettingsWindow>();
        viewFactory.Register<ScriptExecutionWizardViewModel, ScriptExecutionWizard>();

        DispatcherUnhandledException += (s, e) => e.Handled = callbacks.WarnOnUnhandledException(e.Exception);
    }

    private async Task StartConsole(IEnumerable<string> args)
    {
        // Attach or create the console.
        if (!Kernel32.AttachConsole(Kernel32.ATTACH_PARENT_PROCESS))
        {
            _ = Win32Error.ThrowLastErrorIfFalse(Kernel32.AllocConsole());
        }

        // Get invariant (en-US) I/O.
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        // Enable logging in all circumstances.
        ServiceProvider.Get<ISettings>().IsLoggingEnabled = true;

        Logger = new ConsoleLogger();

        Initialize(consoleCallbacks);
        await CheckForUpdates(consoleCallbacks);
        await LoadScripts(consoleCallbacks);

        Environment.ExitCode = new Parser(s =>
        {
            s.CaseInsensitiveEnumValues = true;
            s.CaseSensitive = false;
            s.HelpWriter = Console.Error;
            s.ParsingCulture = CultureInfo.CurrentCulture;
        }).ParseArguments<CommandLineOptions>(args)
            .MapResult(options => options.Execute(ServiceProvider.Get<IScriptStorage>().Scripts), _ => 1);
    }

    private Task StartGui()
    {
        Logger = new CsvLogger();
        Initialize(uiCallbacks);
        MainWindow mainWindow = new() { DataContext = new MainViewModel() };
        mainWindow.Show();
        _ = CheckForUpdates(uiCallbacks);
        return LoadScripts(uiCallbacks);
    }

    private sealed record Callbacks(
        Action<string> NotifyUpdateAvailable,
        ScriptDeserializationErrorCallback ScriptLoadError,
        FSErrorCallback FSErrorReloadElseIgnore,
        UnhandledExceptionCallback WarnOnUnhandledException);
}