using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.ViewModel;

public sealed class ScriptSelection : ObservableObject
{
    private Capability? _desiredCapability = Capability.Enable;
    private bool _isSelected;

    public Capability? DesiredCapability
    {
        get => _desiredCapability;
        set
        {
            _desiredCapability = value;
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
        }
    }
}