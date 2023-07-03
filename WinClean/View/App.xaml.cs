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

    private void ApplicationExit(object? sender, ExitEventArgs? e) => Logs.Exiting.Log();

    private async void ApplicationStartup(object? sender, StartupEventArgs? e)
    {
        if (e?.Args.Any() ?? false)
        {
            await StartConsole(e.Args);
        }
        else
        {
            await StartGui();
            // don't save changed settings in console mode.
            ServiceProvider.Get<ISettings>().Save();
            Logs.SettingsSaved.Log();
        }

        Shutdown();
    }

    private async Task Initialize(Callbacks callbacks)
    {
        // WPF trick thant makes text rendering slightly better on desktop platforms
        TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(TextFormattingMode.Display,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        {
            var viewFactory = ServiceProvider.Get<IViewFactory>();
            viewFactory.Register<AboutViewModel, AboutWindow>();
            viewFactory.Register<SettingsViewModel, SettingsWindow>();
            viewFactory.Register<ScriptExecutionWizardViewModel, ScriptExecutionWizard>();
        }

        DispatcherUnhandledException += (s, e) => e.Handled = callbacks.WarnOnUnhandledException(e.Exception);

        ServiceProvider.Get<IScriptStorage>().Load(callbacks.ScriptLoadError, callbacks.FSErrorReloadElseIgnore);

        var latestVersion = (await SourceControlClient.Instance).LatestVersionName;
        var currentVersion = ServiceProvider.Get<IApplicationInfo>().Version;
        if (ServiceProvider.Get<ISettings>().ShowUpdateDialog && latestVersion != currentVersion)
        {
            Logs.UpdateAvailable.FormatWith(latestVersion, currentVersion).Log(LogLevel.Info);
            await callbacks.NotifyUpdateAvailable();
        }
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

        await Initialize(consoleCallbacks);

        Environment.ExitCode = new Parser(s =>
        {
            s.CaseInsensitiveEnumValues = true;
            s.CaseSensitive = false;
            s.HelpWriter = Console.Error;
            s.ParsingCulture = CultureInfo.CurrentCulture;
        }).ParseArguments<CommandLineOptions>(args)
            .MapResult(options => options.Execute(ServiceProvider.Get<IScriptStorage>().Scripts), _ => 1);
    }

    private async Task StartGui()
    {
        Logger = new CsvLogger();
        await Initialize(uiCallbacks);
        _ = new MainWindow() { DataContext = new MainViewModel() }.ShowDialog();
    }

    private sealed record Callbacks(
        Func<Task> NotifyUpdateAvailable,
        ScriptDeserializationErrorCallback ScriptLoadError,
        FSErrorCallback FSErrorReloadElseIgnore,
        UnhandledExceptionCallback WarnOnUnhandledException);
}