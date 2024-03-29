﻿using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.ViewModel;

public sealed class ScriptActionViewModel : ObservableObject, IEquatable<ScriptActionViewModel?>
{
    public ScriptActionViewModel(ScriptAction model)
    {
        Model = model;
        SuccessExitCodes = new(model.SuccessExitCodes);
        SuccessExitCodes.SendUpdatesTo(model.SuccessExitCodes);
        SuccessExitCodes.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SuccessExitCodes));
    }

    public string Code
    {
        get => Model.Code;
        set
        {
            Model.Code = value;
            OnPropertyChanged();
        }
    }

    public Host Host
    {
        get => Model.Host;
        set
        {
            Model.Host = value;
            OnPropertyChanged();
        }
    }

    public int Order
    {
        get => Model.Order;
        set
        {
            Model.Order = value;
            OnPropertyChanged();
        }
    }

    public ObservableSet<int> SuccessExitCodes { get; }
    internal ScriptAction Model { get; }

    public override bool Equals(object? obj) => Equals(obj as ScriptActionViewModel);

    public bool Equals(ScriptActionViewModel? other) => other is not null && Model.Equals(other.Model);

    public override int GetHashCode() => HashCode.Combine(Model);
}