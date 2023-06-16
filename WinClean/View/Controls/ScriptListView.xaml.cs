using System.Windows;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptListView
{
    public static readonly DependencyProperty SelectedScriptProperty = DependencyProperty.Register(nameof(SelectedScript), typeof(ScriptViewModel), typeof(ScriptListView));

    public static readonly DependencyProperty UsageProperty = DependencyProperty.Register(nameof(Usage), typeof(Usage), typeof(ScriptListView));

    public ScriptListView() => InitializeComponent();

    public ScriptViewModel SelectedScript
    {
        get => (ScriptViewModel)GetValue(SelectedScriptProperty);
        set => SetValue(SelectedScriptProperty, value);
    }

    public Usage Usage
    {
        get => (Usage)GetValue(UsageProperty);
        set => SetValue(UsageProperty, value);
    }
}