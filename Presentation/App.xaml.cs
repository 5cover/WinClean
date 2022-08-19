global using System.IO;
using System.Windows;

using Ookii.Dialogs.Wpf;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

/// <summary>
/// This class statically stores data only available to the <see cref="Presentation"/> layer. It also handles the startup /
/// shutdown strategy.
/// </summary>
public partial class App
{
    private static readonly InvalidScriptDataCallback _invalidScriptDataCallback = (e, path) =>
    {
        string filename = Path.GetFileName(path);
        Logs.InvalidScriptData.FormatWith(filename).Log(LogLevel.Error);

        using Dialog invalidScriptData = DialogFactory.MakeInvalidScriptDataDialog(e, path, Button.DeleteScript, Button.Retry, Button.Ignore);
        invalidScriptData.DefaultButton = Button.Retry;
        using Dialog deleteScript = DialogFactory.MakeScriptDeletionConfirmation();

        invalidScriptData.SetConfirmation(Button.DeleteScript, () => deleteScript.ShowDialog() == Button.Yes);

        Button? result = invalidScriptData.ShowDialog();
        if (result == Button.DeleteScript)
        {
            File.Delete(path);
            Logs.ScriptDeleted.FormatWith(filename).Log();
        }

        return result == Button.Retry;
    };

    public static ScriptCollection Scripts { get; } = ScriptCollection.LoadScripts(AppDirectory.ScriptsDir, _invalidScriptDataCallback);

    private static void ShowUnhandledExceptionDialog(Exception e)
    {
        Happenings.Exception.SetAsHappening();
        Logs.UnhandledException.FormatWith(e).Log(LogLevel.Critical);
        using Dialog unhandledExceptionDialog = new(Button.Exit, Button.CopyDetails)
        {
            MainIcon = TaskDialogIcon.Error,
            Content = WinClean.Resources.UI.Dialogs.UnhandledExceptionDialogContent.FormatWith(e.Message),
            ExpandedInformation = e.ToString(),
            EnableHyperlinks = true
        };
        unhandledExceptionDialog.SetConfirmation(Button.CopyDetails, () =>
        {
            Clipboard.SetText(e.ToString());
            return false;
        });
        unhandledExceptionDialog.HyperlinkClicked += (_, args) => Helpers.Open(args.Href);
        unhandledExceptionDialog.ShowDialog();
    }

    private static void StartConsole(string[] args)
    {
        WarnIfNewUpdateAvailable(() => Console.WriteLine($"A new version {SourceControlClient.Instance.Value.LatestVersionName} is available. Consider updating to get new scripts and new features.\r\n" +
            $"Download the new version at {SourceControlClient.Instance.Value.LatestVersionName}"));
        //Environment.ExitCode = new CommandLineInterpreter(args).Execute();
    }

    private static void StartGui()
    {
        WarnIfNewUpdateAvailable(() =>
        {
            Dialog d = new(Button.OK)
            {
                MainInstruction = "New version available",
                Content = $"Version {SourceControlClient.Instance.Value.LatestVersionName} is available. Consider updating to get new scripts and features. <A>See on GitHub</A>",
                AllowDialogCancellation = true,
                MinimizeBox = true,
                MainIcon = TaskDialogIcon.Information,
                EnableHyperlinks = true,
            };
            d.HyperlinkClicked += (_, e) => Helpers.Open(SourceControlClient.Instance.Value.LatestVersionUrl);

            _ = d.Show();
        });

        AppInfo.ReadAppFileRetryOrFail = (ex, verb, info) =>
        {
            using FSErrorDialog dialog = new(ex, verb, info, Button.Retry, Button.Exit);
            return dialog.ShowDialog() == Button.Retry;
        };

        Current.DispatcherUnhandledException += (_, args) => ShowUnhandledExceptionDialog(args.Exception);
        Happenings.Start.SetAsHappening();
        Logs.Started.Log();
        new MainWindow().Show();
    }

    private static void WarnIfNewUpdateAvailable(Action warnNewUpdateAvailable)
    {
        try
        {
            string latestVersion = SourceControlClient.Instance.Value.LatestVersionName;
            if (latestVersion != AppInfo.Version)
            {
                warnNewUpdateAvailable();
            }
        }
        catch (AggregateException)
        {
            // Network API init error. Assume that we have the latest version.
        }
    }

    private void ApplicationExit(object? sender, ExitEventArgs? e)
    {
        Scripts.Save();
        AppInfo.Settings.Save();

        Happenings.Exit.SetAsHappening();
        Logs.Exiting.Log();
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