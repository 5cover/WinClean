using System.Globalization;
using System.Windows;
using System.Windows.Media;

using CommandLine;

using Scover.WinClean.Model;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.View.Pages;
using Scover.WinClean.View.Windows;
using Scover.WinClean.ViewModel.Logging;
using Scover.WinClean.ViewModel.Pages;
using Scover.WinClean.ViewModel.Windows;

using Vanara.PInvoke;

namespace Scover.WinClean.ViewModel;

/// <summary>Handles the startup / shutdown strategy and holds data related to the View layer.</summary>
public sealed partial class App
{
    public static Logger Logger { get; private set; } = new MockLogger();

    private static async Task Initialize(Callbacks callbacks)
    {
        // WPF trick thant makes text rendering slightly better on desktop platforms
        TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(TextFormattingMode.Display,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        var viewResolver = ServiceProvider.Get<IViewResolver>();

        viewResolver.RegisterView<AboutViewModel, AboutWindow>();
        viewResolver.RegisterView<SettingsViewModel, SettingsWindow>();
        viewResolver.RegisterView<ScriptExecutionWizardViewModel, ScriptExecutionWizard>();

        viewResolver.RegisterView<Page1ViewModel, Page1>();
        viewResolver.RegisterView<Page1AViewModel, Page1A>();
        viewResolver.RegisterView<Page2ViewModel, Page2>();
        viewResolver.RegisterView<Page3ViewModel, Page3>();

        Current.DispatcherUnhandledException += (s, e) => e.Handled = callbacks.WarnOnUnhandledException(e.Exception);

        ServiceProvider.Get<IScriptStorage>().Load(callbacks.InvalidScriptData, callbacks.FSErrorReloadElseIgnore);

        if (ServiceProvider.Get<ISettings>().ShowUpdateDialog
            && (await SourceControlClient.Instance).LatestVersionName != ServiceProvider.Get<IApplicationInfo>().Version)
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
            .WithParsed(options => Environment.ExitCode = options.Execute(ServiceProvider.Get<IScriptStorage>().Scripts));

        Current.Shutdown();
    }

    private static async Task StartGui()
    {
        Logger = new CsvLogger();
        await Initialize(uiCallbacks);
        new MainWindow() { DataContext = new MainViewModel() }.Show();
    }

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        ServiceProvider.Get<ISettings>().Save();
        Logs.SettingsSaved.Log();
        Logs.Exiting.Log();
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

    private sealed record Callbacks(
        Func<Task> NotifyUpdateAvailable,
        InvalidScriptDataCallback InvalidScriptData,
        FSErrorCallback FSErrorReloadElseIgnore,
        UnhandledExceptionCallback WarnOnUnhandledException);
}