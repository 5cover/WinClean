using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.Input;

using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptView : INotifyPropertyChanged
{
    public static readonly DependencyProperty DeleteScriptCommandProperty
        = DependencyProperty.Register(nameof(DeleteScriptCommand), typeof(IRelayCommand), typeof(ScriptView));

    public static readonly DependencyProperty IsReadOnlyProperty
           = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ScriptView));

    public ScriptView() => InitializeComponent();

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool AllowEdit { get; private set; }

    public IRelayCommand? DeleteScriptCommand
    {
        get => (IRelayCommand?)GetValue(DeleteScriptCommandProperty);
        set => SetValue(DeleteScriptCommandProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        AllowEdit = !IsReadOnly && newContent is ScriptViewModel { Type.IsMutable: true };
        PropertyChanged?.Invoke(this, new(nameof(AllowEdit)));
        DeleteScriptCommand?.NotifyCanExecuteChanged();
    }

    private void TextBoxVersionsTextChanged(object sender, TextChangedEventArgs e)
        // Perform validation on each keystroke, but don't update the binding.
        => ((TextBox)sender).GetBindingExpression(TextBox.TextProperty)?.ValidateWithoutUpdate();
}