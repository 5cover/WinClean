﻿using System.Collections.ObjectModel;
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
using Scover.WinClean.Resources.UI;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;

namespace Scover.WinClean.ViewModel.Windows;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly Lazy<PropertyInfo[]> _scriptProperties = new(() => typeof(ScriptViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance));

    [ObservableProperty]
    private ScriptViewModel? _selectedScript;

    public MainViewModel()
    {
        {
            ObservableCollection<ScriptViewModel> scripts = new(ScriptStorage.Scripts.Select(CreateScriptViewModel));

            ScriptStorage.Scripts.SendUpdatesTo(scripts, converter: CreateScriptViewModel);
            scripts.SendUpdatesTo(ScriptStorage.Scripts, converter: s => s.Model);

            scripts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(FormattedScriptCount));

            Scripts = new(new CollectionViewSource
            {
                Source = scripts,
                GroupDescriptions =
                {
                    new PropertyGroupDescription(nameof(ScriptViewModel.Category)).SortedBy(nameof(CollectionViewGroup.Name)),
                    new PropertyGroupDescription(nameof(ScriptViewModel.Usages)).SortedBy(nameof(CollectionViewGroup.Name)),
                },
            });
        }

        CheckScriptsByProperty = new RelayCommand<object>(expectedPropertyValue => SelectScripts(
            s => expectedPropertyValue.NotNull()
                .Equals(_scriptProperties.Value.Single(p => p.PropertyType == expectedPropertyValue.GetType()).GetValue(s))));

        CheckAllScripts = new RelayCommand(() => SelectScripts(_ => true));
        UncheckAllScripts = new RelayCommand(() => SelectScripts(_ => false));

        AddScripts = new RelayCommand(() =>
        {
            var lastAddedScript = Option.None<ScriptViewModel>();

            IFilterBuilder builder = ServiceProvider.Get<IFilterBuilder>();

            var filter = builder.Combine(builder.Make(
                new ExtensionGroup(Settings.ScriptFileExtension)),
                builder.Make(MainWindow.AllFiles, ".*"));

            var paths = ServiceProvider.Get<IDialogCreator>().ShowOpenFileDialog(filter, Settings.ScriptFileExtension,
                                                                                 multiselect: true, readonlyChecked: true);

            foreach (var path in paths)
            {
                RetrieveNewScript(path).MatchSome(s =>
                {
                    Scripts.Source.Add(s);
                    Logs.ScriptAdded.FormatWith(path, s.InvariantName).Log();
                    lastAddedScript = s.Some();
                });
            }

            lastAddedScript.MatchSome(s => SelectedScript = s);
        });

        DeleteCurrentScript = new RelayCommand(() =>
        {
            if (DialogFactory.ShowConfirmation(DialogFactory.MakeConfirmScriptDeletion))
            {
                _ = Scripts.Source.Remove(SelectedScript.NotNull());
            }
        }, () => SelectedScript?.Type.IsMutable ?? false);

        ExecuteScripts = new RelayCommand(() =>
        {
            using var executionInfos = Scripts.Where(s => s.Selection.IsSelected)
                .Select(s => s.TryCreateExecutionInfo())
                .WhereSome().OrderBy(executionInfo => executionInfo.Action.Order).ToDisposableEnumerable();

            if (executionInfos.Any())
            {
                ScriptExecutionWizardViewModel viewModel = new(executionInfos.ToList());
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
            _ = new Dialog(noScriptsSelected).ShowDialog();
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

    public IRelayCommand CheckAllScripts { get; }

    public IRelayCommand<object> CheckScriptsByProperty { get; }

    public IAsyncRelayCommand ClearLogs { get; } = new AsyncRelayCommand(Logging.Logging.Logger.ClearLogsAsync);

    public IRelayCommand DeleteCurrentScript { get; }
    public IRelayCommand ExecuteScripts { get; }

    public string FormattedScriptCount => MainWindow.MsgScriptCount.FormatMessage(new()
    {
        ["scriptCount"] = Scripts.Source.Count,
    });

    public IRelayCommand OpenCustomScriptsDir { get; } = new RelayCommand(AppDirectory.Scripts.Open);

    public IRelayCommand OpenLogsDir { get; } = new RelayCommand(AppDirectory.Logs.Open);

    public IRelayCommand OpenOnlineWiki { get; } = new RelayCommand(Settings.WikiUrl.Open);
    public IRelayCommand ReportIssue { get; } = new RelayCommand(ServiceProvider.Get<ISettings>().NewIssueUrl.Open);

    public CollectionWrapper<ObservableCollection<ScriptViewModel>, ScriptViewModel> Scripts { get; }

    public IRelayCommand ShowAboutWindow { get; } = new RelayCommand(() => _ = ServiceProvider.Get<IDialogCreator>().ShowDialog(new AboutViewModel()));

    public IRelayCommand ShowSettingsWindow { get; } = new RelayCommand(() => _ = ServiceProvider.Get<IDialogCreator>().ShowDialog(new SettingsViewModel()));

    public IRelayCommand UncheckAllScripts { get; }

    private static IScriptStorage ScriptStorage => ServiceProvider.Get<IScriptStorage>();

    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    private static bool PromptScriptOverwrite(Script existingScript) => DialogFactory.ShowConfirmation(() => new Page
    {
        WindowTitle = ServiceProvider.Get<IApplicationInfo>().Name,
        Icon = DialogIcon.Warning,
        Content = Resources.UI.Dialogs.ConfirmScriptOverwriteContent.FormatWith(existingScript.Name),
        Buttons = { Button.Yes, Button.No },
    });

    private ScriptViewModel CreateScriptViewModel(Script script)
    {
        ScriptViewModel scriptViewModel = new(script);
        scriptViewModel.PropertyChanged += (_, e) =>
        {
            bool aGroupingCriterionChanged = Scripts.NotNull().View.GroupDescriptions.OfType<PropertyGroupDescription>().Any(g => g.PropertyName == e.PropertyName);
            if (aGroupingCriterionChanged)
            {
                Scripts.View.Refresh();
            }
        };

        if (script.Type.IsMutable)
        {
            scriptViewModel.PropertyChanged += (_, _) => ScriptStorage.Commit(scriptViewModel.Model);
        }

        return scriptViewModel;
    }

    private Option<ScriptViewModel> RetrieveNewScript(string path)
    {
        bool retry;
        do
        {
            try
            {
                var script = ScriptStorage.RetrieveScript(ScriptType.Custom, path);

                if (ScriptStorage.Scripts.FirstOrDefault(s => s.InvariantName == script.InvariantName) is { } existingScript)
                {
                    Logs.ScriptAlreadyExistsCannotAdd.FormatWith(path, existingScript.InvariantName).Log();
                    retry = PromptScriptOverwrite(existingScript);
                    if (retry)
                    {
                        Logs.ScriptOverwritten.FormatWith(path, existingScript.InvariantName).Log(LogLevel.Info);
                        _ = ScriptStorage.Scripts.Remove(existingScript);
                    }
                }
                else
                {
                    return CreateScriptViewModel(script).Some();
                }
            }
            catch (DeserializationException e)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                using Page page = DialogFactory.MakeScriptLoadError(e, path, new() { Button.TryAgain, Button.Ignore });
                retry = Button.TryAgain.Equals(new Dialog(page).ShowDialog());
            }
            catch (Exception e) when (e is FileSystemException or ArgumentException)
            {
                Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
                //                                              avoid nesting FileSystemException
                using Page fsErrorPage = DialogFactory.MakeFSError(e as FileSystemException ?? new FileSystemException(e, FSVerb.Access, path), new() { Button.TryAgain, Button.Ignore });
                fsErrorPage.MainInstruction = Resources.UI.Dialogs.FSErrorAddingCustomScriptMainInstruction;
                retry = Button.TryAgain.Equals(new Dialog(fsErrorPage).ShowDialog());
            }
        } while (retry);
        return Option.None<ScriptViewModel>();
    }

    private void SelectScripts(Predicate<ScriptViewModel> check)
    {
        foreach (var script in Scripts)
        {
            bool checkScript = check(script);
            var capabilities = script.Actions.Keys;
            script.Selection.IsSelected = checkScript;
            script.Selection.DesiredCapability = checkScript
                ? capabilities.FirstOrDefault(c => c.CorrespondingSelectionState is CapabilityCorrespondingSelectionState.Selected)
                : capabilities.FirstOrDefault(c => c.CorrespondingSelectionState is CapabilityCorrespondingSelectionState.Unselected);
        }
    }
}