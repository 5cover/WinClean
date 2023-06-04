using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Services;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.ViewModel;

public class ScriptViewModel : ObservableObject, IEquatable<ScriptViewModel?>, IEquatable<Script?>
{
    private bool _isSelected;
    private Capability? _desiredCapability;

    public ScriptViewModel(Script model)
    {
        Model = model;
        Code = new(model.Code);
        SelectedCode = model.Code.FirstOrDefault();
    }

    [Bindable(false)] // Hide from binding auto-completion
    public Script Model { get; }
    public KeyValuePair<Capability, ScriptAction>? SelectedCode { get; set; }
    public Category Category
    {
        get => Model.Category;
        set
        {
            Model.Category = value;
            OnPropertyChanged();
        }
    }
    public ScriptCodeViewModel Code { get; }
    public string Description
    {
        get => Model.LocalizedDescription[CurrentUICulture];
        set => Model.LocalizedDescription[CurrentUICulture] = value;
    }
    public TimeSpan ExecutionTime
    {
        get => Settings.ScriptExecutionTimes.TryGetValue(InvariantName, out var time) ? time : Settings.ScriptTimeout;
        set
        {
            if (!Settings.ScriptExecutionTimes.TryAdd(InvariantName, value))
            {
                Settings.ScriptExecutionTimes[InvariantName] = value;
            }
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
    public bool IsIncompatibleWithCurrentVersion => !SemVersion.FromVersion(Environment.OSVersion.Version.WithoutRevision()).Satisfies(Model.Versions);
    public string Name { get => Model.Name; set => Model.Name = value; }
    public SafetyLevel SafetyLevel
    {
        get => Model.SafetyLevel;
        set
        {
            Model.SafetyLevel = value;
            OnPropertyChanged();
        }
    }

    public Usage Usage => Usage.Get(Model);

    public ScriptType Type => Model.Type;
    public SemVersionRange Versions
    {
        get => Model.Versions;
        set
        {
            Model.Versions = value;
            OnPropertyChanged();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DesiredAction));
        }
    }

    public Capability? DesiredCapability
    {
        get => _desiredCapability;
        set
        {
            _desiredCapability = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DesiredAction));
        }
    }

    public ScriptAction? DesiredAction =>
        IsSelected && // The script is selecte
        DesiredCapability is not null && // A capability has be choosen
        Code.TryGetValue(DesiredCapability, out var action) && // The capability exists
        DesiredCapability != Code.EffectiveCapability // The capability is different from the effective capability.
            ? action : null;
    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    public override bool Equals(object? obj) => Equals(obj as ScriptViewModel);

    public bool Equals(ScriptViewModel? other) => other is not null && Model.Equals(other.Model);

    public bool Equals(Script? other) => Model.Equals(other);

    public override int GetHashCode() => Model.GetHashCode();
}