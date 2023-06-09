﻿using System.ComponentModel;

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
    private KeyValuePair<Capability, ScriptAction> _selectedCode;

    public ScriptViewModel(Script model)
    {
        Model = model;
        Code = new(model.Code);
        SelectedCode = model.Code.First();
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

    public ScriptCodeViewModel Code { get; }

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

    [Bindable(false)] // Hide from binding auto-completion
    public Script Model { get; }

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

    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    public Option<ExecutionInfoViewModel> CreateExecutionInfo() =>
           Selection.DesiredCapability is { } desiredCapability && // A capability has be choosen
        Code.TryGetValue(desiredCapability, out var action) && // The capability exists
        !desiredCapability.Equals(Code.EffectiveCapability) // The capability is different from the effective capability.
        ? new ExecutionInfoViewModel(this, desiredCapability, action).Some() : Option.None<ExecutionInfoViewModel>();

    public override bool Equals(object? obj) => Equals(obj as ScriptViewModel);

    public bool Equals(ScriptViewModel? other) => other is not null && Model.Equals(other.Model);

    public bool Equals(Script? other) => Model.Equals(other);

    public override int GetHashCode() => Model.GetHashCode();
}