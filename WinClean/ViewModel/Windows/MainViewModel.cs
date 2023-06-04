using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

using static Scover.WinClean.Resources.UI.Dialogs;

using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;
using Script = Scover.WinClean.Resources.Script;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class MainViewModel : ObservableObject
{
    private readonly Lazy<PropertyInfo[]> _scriptProperties = new(typeof(Script).GetProperties(BindingFlags.Public | BindingFlags.Instance));

    public MainViewModel()
    {
        var scripts = ServiceProvider.Get<IScriptStorage>().Scripts.Select(s => new ScriptViewModel(s));

        Scripts = new(new CollectionViewSource()
        {
            Source = new ObservableCollection<ScriptViewModel>(scripts),
            GroupDescriptions =
            {
                new PropertyGroupDescription($"{nameof(ScriptViewModel.Category)}"),
                new PropertyGroupDescription($"{nameof(ScriptViewModel.Usage)}"),
                new PropertyGroupDescription($"{nameof(ScriptViewModel.Type)}.{nameof(ScriptType.Name)}"),
            },
        });

        Scripts.View.CurrentChanged += (s, e) =>
        {
            // chaud
        };

        UpdateCurrentScriptCategory = new RelayCommand(() =>
        {
            // chaud
        });

        SaveScripts = new RelayCommand(() =>
        {
            Logs.ScriptsSaved.Log();
            ServiceProvider.Get<IScriptStorage>().Save(Scripts.Source.Select(svm => svm.Model));
        });

        CheckScriptsByProperty = new RelayCommand<object>(expectedPropertyValue => SelectScripts(s =>
        {
            ArgumentNullException.ThrowIfNull(expectedPropertyValue);
            object? scriptPropertyValue = _scriptProperties.Value.Single(p => p.PropertyType == expectedPropertyValue.GetType()).GetValue(s);
            return expectedPropertyValue.Equals(scriptPropertyValue);
        }));

        CheckAllScripts = new RelayCommand(() => SelectScripts(_ => true));
        UncheckAllScripts = new RelayCommand(() => SelectScripts(_ => false));

        AddScripts = new RelayCommand(() =>
        {
            var oldScriptCount = Scripts.Source.Count;
            var scriptFileExtension = ServiceProvider.Get<ISettings>().ScriptFileExtension;

            foreach (string path in ServiceProvider.Get<IDialogCreator>().ShowOpenFileDialog(new ExtensionGroup(scriptFileExtension), scriptFileExtension, true, true))
            {
                AddScript(path);
            }

            if (Scripts.Source.Count > oldScriptCount)
            {
                _ = Scripts.View.MoveCurrentToLast();
            }
        });

        RemoveCurrentScript = new RelayCommand(() => Scripts.RemoveCurrentItem());

        ExecuteScripts = new RelayCommand(() =>
        {
            var executionInfos = Scripts.Source
                .Select(s => s.DesiredCapability is not null && s.DesiredAction is not null ? new ExecutionInfoViewModel(s, s.DesiredCapability, s.DesiredAction) : null)
                .WithoutNull().ToList();

            if (executionInfos.Any())
            {
                using ScriptExecutionWizardViewModel viewModel = new(executionInfos);
                _ = ServiceProvider.Get<IDialogCreator>().ShowDialog(viewModel);
                return;
            }

            using Page noScriptsSelected = new()
            {
                IsCancelable = true,
                WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
                Icon = DialogIcon.Error,
                MainInstruction = NoScriptsSelectedMainInstruction,
                Content = NoScriptsSelectedContent,
            };
            _ = new Dialog(noScriptsSelected).Show();
        });
    }

    public static string ApplicationName => ServiceProvider.Get<IApplicationInfo>().Name;
    public static double Height
    {
        get => ServiceProvider.Get<ISettings>().Height;
        set => ServiceProvider.Get<ISettings>().Height = value;
    }
    public static double Left
    {
        get => ServiceProvider.Get<ISettings>().Left;
        set => ServiceProvider.Get<ISettings>().Left = value;
    }
    public static TypedEnumerableDictionary Metadatas => ServiceProvider.Get<IMetadatasProvider>().Metadatas;
    public static int ScriptCount => ServiceProvider.Get<IScriptStorage>().ScriptCount;
    public static double Top
    {
        get => ServiceProvider.Get<ISettings>().Top;
        set => ServiceProvider.Get<ISettings>().Top = value;
    }
    public static double Width
    {
        get => ServiceProvider.Get<ISettings>().Width;
        set => ServiceProvider.Get<ISettings>().Width = value;
    }
    public static WindowState WindowState
    {
        get => ServiceProvider.Get<ISettings>().IsMaximized ? WindowState.Maximized : WindowState.Normal;
        set => ServiceProvider.Get<ISettings>().IsMaximized = value is WindowState.Maximized;
    }
    public CollectionWrapper<ObservableCollection<ScriptViewModel>, ScriptViewModel> Scripts { get; }
    public IRelayCommand AddScripts { get; }
    public IRelayCommand CheckAllScripts { get; }
    public IRelayCommand<object> CheckScriptsByProperty { get; }
    public IAsyncRelayCommand ClearLogs { get; } = new AsyncRelayCommand(App.Logger.ClearLogs);
    public IRelayCommand ExecuteScripts { get; }
    public IRelayCommand OpenCustomScriptsDir { get; } = new RelayCommand(() => AppDirectory.Scripts.Open());
    public IRelayCommand OpenLogsDir { get; } = new RelayCommand(() => AppDirectory.Logs.Open());
    public IAsyncRelayCommand OpenOnlineWiki { get; } = new AsyncRelayCommand(async () => (await SourceControlClient.Instance).WikiUrl.Open());
    public IRelayCommand UpdateCurrentScriptCategory { get; }
    public IRelayCommand RemoveCurrentScript { get; }
    public IRelayCommand SaveScripts { get; }
    public IRelayCommand ShowAboutWindow { get; } = new RelayCommand(() => ServiceProvider.Get<IDialogCreator>().ShowDialog(new AboutViewModel()));
    public IRelayCommand ShowSettingsWindow { get; } = new RelayCommand(() => ServiceProvider.Get<IDialogCreator>().ShowDialog(new SettingsViewModel()));
    public IRelayCommand UncheckAllScripts { get; }

    public static bool AskUserToOverwriteScript(ScriptViewModel existingScript)
    {
        using Page overwrite = new()
        {
            WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
            Icon = DialogIcon.Warning,
            Content = ConfirmScriptOverwriteContent.FormatWith(existingScript.Name),
            Buttons = { Button.Yes, Button.No },
        };
        return Button.Yes.Equals(new Dialog(overwrite).Show(ParentWindow.Active));
    }

    private void AddScript(string path)
    {
        bool retry = true;
        while (retry)
        {
            try
            {
                using var file = File.OpenRead(path);
                ScriptViewModel script = new(ServiceProvider.Get<IScriptStorage>().Serializer.Deserialize(ScriptType.Custom, file));
                // If the script already exists
                if (Scripts.Source.Contains(script))
                {
                    Logs.ScriptAlreadyExistsCannotBeAdded.FormatWith(path, script.InvariantName).Log();
                    retry = AskUserToOverwriteScript(script);
                    if (retry)
                    {
                        // If the user chose to overwrite, delete the already existing script.
                        Debug.Assert(Scripts.Source.Remove(script));
                    }
                }
                else
                {
                    Logs.ScriptAdded.FormatWith(path, script.InvariantName).Log();
                    Scripts.Source.Add(script);
                    retry = false;
                }
            }
            catch (InvalidDataException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page page = DialogPages.InvalidScriptData(e, path, new() { Button.Retry, Button.Ignore });
                retry = Button.Retry.Equals(new Dialog(page).Show());
            }
            catch (Exception e) when (e.IsFileSystemExogenous())
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page fsErrorPage = DialogPages.FSError(new FileSystemException(e, FSVerb.Access, path), new() { Button.Retry, Button.Ignore });
                fsErrorPage.MainInstruction = FSErrorAddingCustomScriptMainInstruction;
                retry = Button.Retry.Equals(new Dialog(fsErrorPage).Show());
            }
        }
    }

    private void SelectScripts(Predicate<ScriptViewModel> check)
    {
        foreach (var script in Scripts.Source)
        {
            script.IsSelected = check(script);
        }
        Scripts.View.Refresh();
    }
}