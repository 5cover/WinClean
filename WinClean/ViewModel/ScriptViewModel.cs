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
    private readonly Script _model;
    private KeyValuePair<Capability, ScriptAction> _selectedCode;

    public ScriptViewModel(Script model)
    {
        _model = model;
        Code = new(model.Code);
        SelectedCode = model.Code.First();
    }

    public Category Category
    {
        get => _model.Category;
        set
        {
            _model.Category = value;
            OnPropertyChanged();
        }
    }

    public ScriptCodeViewModel Code { get; }

    public string Description
    {
        get => _model.LocalizedDescription[CurrentUICulture];
        set => _model.LocalizedDescription[CurrentUICulture] = value;
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
        get => _model.Impact;
        set
        {
            _model.Impact = value;
            OnPropertyChanged();
        }
    }

    public string InvariantName => _model.InvariantName;

    public string Name
    {
        get => _model.Name;
        set
        {
            _model.Name = value;
            OnPropertyChanged();
        }
    }

    public SafetyLevel SafetyLevel
    {
        get => _model.SafetyLevel;
        set
        {
            _model.SafetyLevel = value;
            OnPropertyChanged();
        }
    }

    public KeyValuePair<Capability, ScriptAction> SelectedCode
    {
        get => _selectedCode;
        set
        {
            _selectedCode = value;
            OnPropertyChanged();
        }
    }

    public ScriptSelection Selection { get; } = new();
    public ScriptType Type => _model.Type;
    public IReadOnlyCollection<Usage> Usages => Usage.GetUsages(_model).ToList();

    public SemVersionRange Versions
    {
        get => _model.Versions;
        set
        {
            _model.Versions = value;
            OnPropertyChanged();
        }
    }

    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    private static IScriptStorage Storage => ServiceProvider.Get<IScriptStorage>();

    public override bool Equals(object? obj) => Equals(obj as ScriptViewModel);

    public bool Equals(ScriptViewModel? other) => other is not null && _model.Equals(other._model);

    public bool Equals(Script? other) => _model.Equals(other);

    public override int GetHashCode() => _model.GetHashCode();

    public bool RemoveFromStorage() => Storage.Remove(_model);

    public Option<ExecutionInfoViewModel> TryCreateExecutionInfo() =>
        Selection.DesiredCapability is { } desiredCapability // A capability has be choosen
        && Code.TryGetValue(desiredCapability, out var action) // The capability exists
        ? new ExecutionInfoViewModel(this, desiredCapability, action).Some()
        : Option.None<ExecutionInfoViewModel>();

    public void UpdateInStorage() => Storage.Update(_model);
}