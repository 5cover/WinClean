using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptView : INotifyPropertyChanged
{
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ScriptView));

    public ScriptView() => InitializeComponent();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary><see cref="Script"/> was removed.</summary>
    public event TypeEventHandler<ScriptView>? ScriptRemoved;

    public bool AllowEdit { get; private set; }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        AllowEdit = !IsReadOnly && newContent is ScriptViewModel newScript && newScript.Type.IsMutable;
        PropertyChanged?.Invoke(this, new(nameof(AllowEdit)));
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e) => ScriptRemoved?.Invoke(this, EventArgs.Empty);

    private void TextBoxVersionsTextChanged(object sender, TextChangedEventArgs e)
        // Perform validation on every keystore, but don't update the binding.
        => ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
}