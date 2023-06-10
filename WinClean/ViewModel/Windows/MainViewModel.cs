using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Optional;

using Scover.Dialogs;
using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.View;
using Scover.WinClean.ViewModel.Logging;

using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;
using Script = Scover.WinClean.Resources.Script;

namespace Scover.WinClean.ViewModel.Windows;

public sealed class MainViewModel : ObservableObject
{
    private readonly Lazy<PropertyInfo[]> _scriptProperties = new(typeof(Script).GetProperties(BindingFlags.Public | BindingFlags.Instance));
    private ScriptViewModel? _selectedScript;

    public MainViewModel()
    {
        Scripts = new(ServiceProvider.Get<IScriptStorage>().Scripts.Select(s =>
        {
            ScriptViewModel svm = new(s);
            svm.PropertyChanged += (s, e) =>
            {
                if (ScriptGroups.NotNull().GroupDescriptions.OfType<PropertyGroupDescription>().Any(g => g.PropertyName == e.PropertyName))
                {
                    ScriptGroups.View.Refresh();
                }
            };
            return svm;
        }));

        ScriptGroups = new CollectionViewSource()
        {
            Source = Scripts,
            GroupDescriptions =
            {
                new PropertyGroupDescription(nameof(ScriptViewModel.Category)),
                new PropertyGroupDescription(nameof(ScriptViewModel.Usages)),
            }
        };

        SaveScripts = new RelayCommand(() =>
        {
            ServiceProvider.Get<IScriptStorage>().Save(Scripts.Select(svm => svm.Model));
            Logs.ScriptsSaved.Log();
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
            var scriptFileExtension = ServiceProvider.Get<ISettings>().ScriptFileExtension;
            Option<ScriptViewModel> lastAddedScript = Option.None<ScriptViewModel>();

            var paths = ServiceProvider.Get<IDialogCreator>().ShowOpenFileDialog(new(scriptFileExtension), scriptFileExtension, true, true);

            foreach ((var path, var script) in paths.Select(p => (p, DeserializeNewScript(p))))
            {
                if (ScriptAddStrategy.AddScript(Scripts, path, script))
                {
                    lastAddedScript = script;
                }
            }

            lastAddedScript.MatchSome(s => SelectedScript = s);
        });

        RemoveCurrentScript = new RelayCommand(() =>
        {
            using Page page = DialogPages.ConfirmScriptDeletion();
            if (Button.Yes.Equals(new Dialog(page).Show()))
            {
                Debug.Assert(Scripts.Remove(SelectedScript.NotNull()));
            }
        });

        ExecuteScripts = new RelayCommand(() =>
        {
            if (Scripts.Where(s => s.Selection.IsSelected).Select(s => s.CreateExecutionInfo()).WhereSome().ToList() is { Count: > 0 } executionInfos)
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
                MainInstruction = Resources.UI.Dialogs.NoScriptsSelectedMainInstruction,
                Content = Resources.UI.Dialogs.NoScriptsSelectedContent,
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

    public IRelayCommand AddScripts { get; }
    public IRelayCommand CheckAllScripts { get; }
    public IRelayCommand<object> CheckScriptsByProperty { get; }
    public IAsyncRelayCommand ClearLogs { get; } = new AsyncRelayCommand(App.CurrentApp.Logger.ClearLogsAsync);
    public IRelayCommand ExecuteScripts { get; }
    public IRelayCommand OpenCustomScriptsDir { get; } = new RelayCommand(() => AppDirectory.Scripts.Open());
    public IRelayCommand OpenLogsDir { get; } = new RelayCommand(() => AppDirectory.Logs.Open());
    public IAsyncRelayCommand OpenOnlineWiki { get; } = new AsyncRelayCommand(async () => (await SourceControlClient.Instance).WikiUrl.Open());
    public IRelayCommand RemoveCurrentScript { get; }
    public IRelayCommand SaveScripts { get; }
    public CollectionViewSource ScriptGroups { get; }
    public ObservableCollection<ScriptViewModel> Scripts { get; }

    public ScriptViewModel? SelectedScript
    {
        get => _selectedScript;
        set
        {
            _selectedScript = value;
            OnPropertyChanged();
        }
    }

    public IRelayCommand ShowAboutWindow { get; } = new RelayCommand(() => ServiceProvider.Get<IDialogCreator>().ShowDialog(new AboutViewModel()));

    public IRelayCommand ShowSettingsWindow { get; } = new RelayCommand(() => ServiceProvider.Get<IDialogCreator>().ShowDialog(new SettingsViewModel()));

    public IRelayCommand UncheckAllScripts { get; }

    private static Option<ScriptViewModel> DeserializeNewScript(string path)
    {
        bool retry;
        do
        {
            try
            {
                using var file = File.OpenRead(path);
                ScriptViewModel script = new(ServiceProvider.Get<IScriptStorage>().Serializer.Deserialize(ScriptType.Custom, file));
                Logs.ScriptAdded.FormatWith(path, script.InvariantName).Log();
                return script.Some();
            }
            catch (DeserializationException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page page = DialogPages.ScriptLoadError(e, path, new() { Button.TryAgain, Button.Ignore });
                retry = Button.TryAgain.Equals(new Dialog(page).Show());
            }
            catch (Exception e) when (e.IsFileSystemExogenous())
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page fsErrorPage = DialogPages.FSError(new FileSystemException(e, FSVerb.Access, path), new() { Button.TryAgain, Button.Ignore });
                fsErrorPage.MainInstruction = Resources.UI.Dialogs.FSErrorAddingCustomScriptMainInstruction;
                retry = Button.TryAgain.Equals(new Dialog(fsErrorPage).Show());
            }
        } while (retry);
        return Option.None<ScriptViewModel>();
    }

    private void SelectScripts(Predicate<ScriptViewModel> check)
    {
        foreach (var script in Scripts)
        {
            script.Selection.IsSelected = check(script);
        }
    }

    private sealed class ScriptAddStrategy
    {
        private readonly Func<ICollection<ScriptViewModel>, string, Option<ScriptViewModel>, bool> _add;

        private ScriptAddStrategy(Func<ICollection<ScriptViewModel>, string, Option<ScriptViewModel>, bool> add) => _add = add;

        public static ScriptAddStrategy Add { get; } = new((scripts, path, script) => script.Match(s =>
        {
            scripts.Add(s);
            Logs.ScriptAdded.FormatWith(path, s).Log();
            return true;
        },
        () => false));

        public static ScriptAddStrategy DontAddBecauseAlreadyExists { get; } = new((_, path, script) =>
        {
            script.MatchSome(s => Logs.ScriptAlreadyExistsCannotAdd.FormatWith(path, s.InvariantName).Log(LogLevel.Info));
            return false;
        });

        public static ScriptAddStrategy DontAddBecauseDeserializationFailed { get; } = new((_, path, _) =>
        {
            Logs.ScriptDeserializationFailedCannotAdd.FormatWith(path).Log(LogLevel.Info);
            return false;
        });

        public static ScriptAddStrategy Overwrite { get; } = new((scripts, path, script) => script.Match(s =>
        {
            Debug.Assert(scripts.Remove(s));
            scripts.Add(s);
            Logs.ScriptOverwritten.FormatWith(path, s.InvariantName).Log(LogLevel.Info);
            return true;
        },
        () => false));

        public static bool AddScript(ICollection<ScriptViewModel> scripts, string path, Option<ScriptViewModel> newScript)
        => newScript.Match(s =>
            scripts.Contains(s)
                ? AskUserToOverwriteScript(s)
                    ? Overwrite
                    : DontAddBecauseAlreadyExists
                : Add,
        () => DontAddBecauseDeserializationFailed)._add(scripts, path, newScript);

        private static bool AskUserToOverwriteScript(ScriptViewModel existingScript)
        {
            using Page overwrite = new()
            {
                WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
                Icon = DialogIcon.Warning,
                Content = Resources.UI.Dialogs.ConfirmScriptOverwriteContent.FormatWith(existingScript.Name),
                Buttons = { Button.Yes, Button.No },
            };
            return Button.Yes.Equals(new Dialog(overwrite).Show(ParentWindow.Active));
        }
    }
}