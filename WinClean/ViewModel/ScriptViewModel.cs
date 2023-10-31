using CommunityToolkit.Mvvm.ComponentModel;

using Optional;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Services;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.ViewModel;

public class ScriptViewModel : ObservableObject, IEquatable<ScriptViewModel?>, IEquatable<Script?>
{
    private KeyValuePair<Capability, ScriptAction> _selectedAction;

    public ScriptViewModel(Script model)
    {
        Model = model;
        Actions = new(model.Actions);
        SelectedAction = model.Actions.First();
    }

    public Category Category
    {
        get => Model.Category;
        set
        {
            Model.Category = value;
            OnPropertyChanged();
        }
    }

    public ScriptActionDictionaryViewModel Actions { get; }

    public string Description
    {
        get => Model.LocalizedDescription[CurrentUICulture];
        set => Model.LocalizedDescription[CurrentUICulture] = value;
    }

    public Option<TimeSpan> ExecutionTime
    {
        get => Settings.ScriptExecutionTimes.TryGetValue(InvariantName, out var time) ? time.Some() : time.None();
        set
        {
            value.Match(time => Settings.ScriptExecutionTimes.SetOrAdd(InvariantName, time),
                        () => Settings.ScriptExecutionTimes.Remove(InvariantName));
            OnPropertyChanged();
        }
    }

    public Impact Impact
    {
        get => Model.Impact;
        set
        {
            Model.Impact = value;
            OnPropertyChanged();
        }
    }

    public string InvariantName => Model.InvariantName;

    public string Name
    {
        get => Model.Name;
        set
        {
            Model.Name = value;
            OnPropertyChanged();
        }
    }

    public SafetyLevel SafetyLevel
    {
        get => Model.SafetyLevel;
        set
        {
            Model.SafetyLevel = value;
            OnPropertyChanged();
        }
    }

    public KeyValuePair<Capability, ScriptAction> SelectedAction
    {
        get => _selectedAction;
        set
        {
            _selectedAction = value;
            OnPropertyChanged();
        }
    }

    public ScriptSelection Selection { get; } = new();
    public ScriptType Type => Model.Type;
    public IReadOnlyCollection<Usage> Usages => Usage.GetUsages(Model).ToList();

    public SemVersionRange Versions
    {
        get => Model.Versions;
        set
        {
            Model.Versions = value;
            OnPropertyChanged();
        }
    }

    internal Script Model { get; }
    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    public override bool Equals(object? obj) => Equals(obj as ScriptViewModel);

    public bool Equals(ScriptViewModel? other) => other is not null && Model.Equals(other.Model);

    public bool Equals(Script? other) => Model.Equals(other);

    public override int GetHashCode() => Model.GetHashCode();

    public Option<ExecutionInfoViewModel> TryCreateExecutionInfo() =>
        Selection.DesiredCapability is { } desiredCapability // A capability has be choosen
        && Actions.TryGetValue(desiredCapability, out var action) // The capability exists
        ? new ExecutionInfoViewModel(this, desiredCapability, action).Some()
        : Option.None<ExecutionInfoViewModel>();
}