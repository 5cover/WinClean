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
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.View;
using Scover.WinClean.ViewModel.Logging;

using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;

namespace Scover.WinClean.ViewModel.Windows;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IEnumerable<ScriptViewModel> _orginalScripts;
    private readonly Lazy<PropertyInfo[]> _scriptProperties = new(typeof(ScriptViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance));

    [ObservableProperty]
    private ScriptViewModel? _selectedScript;

    public MainViewModel()
    {
        _orginalScripts = ServiceProvider.Get<IScriptStorage>().Scripts.Select(s =>
        {
            ScriptViewModel svm = new(s);
            svm.PropertyChanged += (s, e) =>
            {
                if (Scripts.NotNull().View.GroupDescriptions.OfType<PropertyGroupDescription>().Any(g => g.PropertyName == e.PropertyName))
                {
                    Scripts.View.Refresh();
                }
            };
            return svm;
        });

        Scripts = new(new CollectionViewSource()
        {
            Source = new ObservableCollection<ScriptViewModel>(_orginalScripts),
            GroupDescriptions =
            {
                new PropertyGroupDescription(nameof(ScriptViewModel.Category)).SortedBy(nameof(CollectionViewGroup.Name)),
                new PropertyGroupDescription(nameof(ScriptViewModel.Usages)).SortedBy(nameof(CollectionViewGroup.Name)),
            },
        });

        ApplyChangesToScriptStorage = new RelayCommand(() =>
        {
            var mutableScripts = Scripts.Source.Where(s => s.Type.IsMutable);

            foreach (var removedScript in _orginalScripts.Where(s => s.Type.IsMutable).Except(mutableScripts))
            {
                _ = removedScript.RemoveFromStorage();
            }
            // Added scripts are hanlded immediately in TryAddNewScript
            foreach (var script in mutableScripts)
            {
                script.UpdateInStorage();
            }

            Logs.ScriptsSaved.Log();
        });

        CheckScriptsByProperty = new RelayCommand<object>(expectedPropertyValue => SelectScripts(s
            => expectedPropertyValue.NotNull().Equals(
                _scriptProperties.Value.Single(p => p.PropertyType == expectedPropertyValue.GetType()).GetValue(s))));

        CheckAllScripts = new RelayCommand(() => SelectScripts(_ => true));
        UncheckAllScripts = new RelayCommand(() => SelectScripts(_ => false));

        AddScripts = new RelayCommand(() =>
        {
            var lastAddedScript = Option.None<ScriptViewModel>();

            IFilterBuilder builder = ServiceProvider.Get<IFilterBuilder>();

            var filter = builder.Combine(builder.Make(
                new ExtensionGroup(Settings.ScriptFileExtension)),
                builder.Make(Resources.UI.MainWindow.AllFiles, ".*"));

            var paths = ServiceProvider.Get<IDialogCreator>().ShowOpenFileDialog(filter, Settings.ScriptFileExtension, true, true);

            foreach (var path in paths)
            {
                var script = TryAddNewScript(path);
                script.MatchSome(s =>
                {
                    Scripts.Source.Add(s);
                    Logs.ScriptAdded.FormatWith(path, s.InvariantName).Log();
                    lastAddedScript = s.Some();
                });
            }

            lastAddedScript.MatchSome(s => SelectedScript = s);
        });

        RemoveCurrentScript = new RelayCommand(() =>
        {
            if (DialogFactory.ShowConfirmation(DialogFactory.MakeConfirmScriptDeletion))
            {
                Debug.Assert(Scripts.Source.Remove(SelectedScript.NotNull()));
            }
        });

        ExecuteScripts = new RelayCommand(() =>
        {
            if (Scripts.Where(s => s.Selection.IsSelected)
                .Select(s => s.TryCreateExecutionInfo())
                .WhereSome().ToList() is { Count: > 0 } executionInfos)
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
    public static double Height { get => Settings.Height; set => Settings.Height = value; }
    public static double Left { get => Settings.Left; set => Settings.Left = value; }
    public static double Top { get => Settings.Top; set => Settings.Top = value; }
    public static double Width { get => Settings.Width; set => Settings.Width = value; }

    public static WindowState WindowState
    {
        get => Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
        set => Settings.IsMaximized = value is WindowState.Maximized;
    }

    public IRelayCommand AddScripts { get; }
    public IRelayCommand ApplyChangesToScriptStorage { get; }
    public IRelayCommand CheckAllScripts { get; }
    public IRelayCommand<object> CheckScriptsByProperty { get; }
    public IAsyncRelayCommand ClearLogs { get; } = new AsyncRelayCommand(App.CurrentApp.Logger.ClearLogsAsync);
    public IRelayCommand ExecuteScripts { get; }
    public IRelayCommand OpenCustomScriptsDir { get; } = new RelayCommand(AppDirectory.Scripts.Open);
    public IRelayCommand OpenLogsDir { get; } = new RelayCommand(AppDirectory.Logs.Open);
    public IAsyncRelayCommand OpenOnlineWiki { get; } = new AsyncRelayCommand(async () => (await SourceControlClient.Instance).WikiUrl.Open());
    public IRelayCommand RemoveCurrentScript { get; }
    public CollectionWrapper<ObservableCollection<ScriptViewModel>, ScriptViewModel> Scripts { get; }

    public IRelayCommand ShowAboutWindow { get; } = new RelayCommand(() => _ = ServiceProvider.Get<IDialogCreator>().ShowDialog(new AboutViewModel()));
    public IRelayCommand ShowSettingsWindow { get; } = new RelayCommand(() => _ = ServiceProvider.Get<IDialogCreator>().ShowDialog(new SettingsViewModel()));
    public IRelayCommand UncheckAllScripts { get; }
    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    private static Option<ScriptViewModel> TryAddNewScript(string path)
    {
        bool retry;
        do
        {
            try
            {
                return new ScriptViewModel(ServiceProvider.Get<IScriptStorage>().Add(ScriptType.Custom, path)).Some();
            }
            catch (DeserializationException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page page = DialogFactory.MakeScriptLoadError(e, path, new() { Button.TryAgain, Button.Ignore });
                retry = Button.TryAgain.Equals(new Dialog(page).Show());
            }
            catch (FileSystemException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page fsErrorPage = DialogFactory.MakeFSError(e, new() { Button.TryAgain, Button.Ignore });
                fsErrorPage.MainInstruction = Resources.UI.Dialogs.FSErrorAddingCustomScriptMainInstruction;
                retry = Button.TryAgain.Equals(new Dialog(fsErrorPage).Show());
            }
            catch (ScriptAlreadyExistsException e)
            {
                Logs.ScriptAlreadyExistsCannotAdd.FormatWith(path, e.ExistingScript.InvariantName).Log();
                retry = DialogFactory.ShowConfirmation(() => new Page()
                {
                    WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
                    Icon = DialogIcon.Warning,
                    Content = Resources.UI.Dialogs.ConfirmScriptOverwriteContent.FormatWith(e.ExistingScript.Name),
                    Buttons = { Button.Yes, Button.No },
                });
                if (retry)
                {
                    Debug.Assert(ServiceProvider.Get<IScriptStorage>().Remove(e.ExistingScript));
                    Logs.ScriptOverwritten.FormatWith(path, e.ExistingScript.InvariantName).Log(LogLevel.Info);
                }
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
}