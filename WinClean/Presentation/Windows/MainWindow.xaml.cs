using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Scover.Dialogs;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;
using static Scover.WinClean.Resources.UI.Dialogs;
using Button = Scover.Dialogs.Button;
using Page = Scover.Dialogs.Page;

namespace Scover.WinClean.Presentation.Windows;

public sealed partial class MainWindow
{
    private readonly Dictionary<Category, Script?> _selectedScripts = new();

    public MainWindow()
    {
        InitializeComponent();
        Top = App.Settings.Top;
        Left = App.Settings.Left;
        Width = App.Settings.Width;
        Height = App.Settings.Height;
        WindowState = App.Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;

        CheckScriptsByMetadata = new(metadata => CheckScripts(s =>
        {
            ScriptMetadata scriptMetadata = metadata switch
            {
                Category => s.Category,
                RecommendationLevel => s.RecommendationLevel,
                Host => s.Host,
                Impact => s.Impact,
                _ => throw new ArgumentException($"'{metadata.AssertNotNull().GetType()}' is an unknown script metadata type", nameof(metadata))
            };
            return scriptMetadata == metadata;
        }));
    }

    public RelayCommand<ScriptMetadata> CheckScriptsByMetadata { get; }

    public ObservableCollection<Script> Scripts { get; } = new(App.Scripts);

    private DataGrid? DataGridScripts => TabControlCategories.FindVisualChildren<DataGrid>().SingleOrDefault();
    private ICollectionView ScriptCollectionView => ((CollectionViewSource)Resources["Scripts"]).View;

    /// <summary>Gets or sets the currently selected script.</summary>
    /// <remarks>Asserts that <see cref="DataGridScripts"/> is not null before setting the selected script.</remarks>
    private Script? SelectedScript
    {
        get => ScriptEditor.Selected;
        // Don't set ScriptEditor.Selected because it wouldn't update DataGridScripts.SelectedItem. Setting
        // DataGridScripts.SelectedItem updates ScriptEditor.Selected due to a OneWayToSource binding in DataGridScripts.
        set => DataGridScripts.AssertNotNull().SelectedItem = value;
    }

    private static bool Handle(string path, InvalidDataException e)
    {
        Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
        return Button.Retry.Equals(new Dialog(DialogPageFactory.MakeInvalidScriptData(e, path, new() { Button.Retry, Button.Ignore })).Show());
    }

    private static bool Handle(string path, Exception e)
    {
        Debug.Assert(e.IsFileSystem());
        Logs.ScriptLoadError.FormatWith(path, e).Log(LogLevel.Error);
        Page fsErrorPage = DialogPageFactory.MakeFSError(e, FSVerb.Access, new FileInfo(path), new() { Button.Retry, Button.Ignore });
        fsErrorPage.MainInstruction = FSErrorAddingCustomScriptMainInstruction;
        return Button.Retry.Equals(new Dialog(fsErrorPage).Show());
    }

    private void AddScript(string path)
    {
        IScriptSerializer serializer = new ScriptXmlSerializer(App.ScriptMetadata);
    retry:
        try
        {
            using Stream file = File.OpenRead(path);
            Script script = serializer.Deserialize(ScriptType.Custom, file);

            if (Scripts.SingleOrDefault(s => s.InvariantName == script.InvariantName) is { } existingScript)
            {
                throw new ScriptAlreadyExistsException(existingScript);
            }
            else
            {
                Scripts.Add(script);
                Logs.ScriptAdded.FormatWith(path, script.InvariantName).Log();
            }
        }
        catch (InvalidDataException e)
        {
            if (Handle(path, e))
            {
                goto retry;
            }
        }
        catch (ScriptAlreadyExistsException e)
        {
            if (Handle(path, e))
            {
                goto retry;
            }
        }
        catch (Exception e) when (e.IsFileSystem())
        {
            if (Handle(path, e))
            {
                goto retry;
            }
        }
    }

    private IEnumerable<string> AskForCustomScriptsToAdd()
    {
        OpenFileDialog ofd = new()
        {
            DefaultExt = App.Settings.ScriptFileExtension,
            Multiselect = true,
            ReadOnlyChecked = true
        };
        MakeFilter(new(App.Settings.ScriptFileExtension));

        return !ofd.ShowDialog(this) ?? true ? Enumerable.Empty<string>() : ofd.FileNames;

        void MakeFilter(ExtensionGroup group)
        {
            string extensions = string.Join(";", group.Select(ext => $"*{ext}"));
            ofd.Filter = $"{group.Name} ({extensions})|{extensions}";
        }
    }

    private void CheckScripts(Predicate<Script> check)
    {
        foreach (var script in Scripts)
        {
            script.IsSelected = check(script);
        }
    }

    private bool Handle(string path, ScriptAlreadyExistsException e)
    {
        Logs.ScriptAlreadyExistsCannotBeAdded.FormatWith(path, e.ExistingScript.InvariantName).Log();
        Page overwrite = new()
        {
            Buttons = new() { Button.Yes, Button.No },
            Content = ConfirmScriptOverwriteContent.FormatWith(e.ExistingScript.Name),
            Icon = DialogIcon.Warning,
        };
        bool retry = Button.Yes.Equals(new Dialog(overwrite).Show());
        if (retry)
        {
            _ = Scripts.Remove(e.ExistingScript);
        }
        return retry;
    }

    private void RefreshScriptFilter(Category category)
    {
        // 1. Commit any pending edits and exit edit mode before setting the filter.
        _ = DataGridScripts?.CommitEdit(DataGridEditingUnit.Row, true);

        // 2. Set the filter to keep only the scripts with the current category.
        ScriptCollectionView.Filter = s => ((Script)s).Category == category;
        // 3. Refresh to apply changes
        ScriptCollectionView.Refresh();

        // 4. Refreshing unfocuses the DataGrid, so focus it.
        _ = DataGridScripts?.Focus();

        // DataGridScripts could be null here because this method may be called right when the window is shown, before all
        // elements have initialized.
        if (DataGridScripts is not null && _selectedScripts.TryGetValue(category, out var selected))
        {
            // 5. Restore the selected item.
            SelectedScript = selected;
        }
    }

    private void RemoveSelectedScript()
    {
        if (Button.Yes.Equals(new Dialog(DialogPageFactory.MakeDeleteScript()).Show()))
        {
            Logs.ScriptRemoved.FormatWith(SelectedScript.AssertNotNull().InvariantName).Log();
            _ = Scripts.Remove(SelectedScript);
            ScriptCollectionView.Refresh();
        }
    }

    private sealed class ScriptAlreadyExistsException : Exception
    {
        public ScriptAlreadyExistsException(Script existingScript)
            : base($"The script '{existingScript.InvariantName}' already exists")
            => ExistingScript = existingScript;

        public Script ExistingScript { get; }
    }
}