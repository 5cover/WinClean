using System.Windows;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptActionDictionaryView
{
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ScriptActionDictionaryView));

    public ScriptActionDictionaryView() => InitializeComponent();

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
}