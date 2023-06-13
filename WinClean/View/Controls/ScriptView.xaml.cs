using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptView : INotifyPropertyChanged
{
    public static readonly DependencyProperty IsReadOnlyProperty
        = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ScriptView));

    public ScriptView() => InitializeComponent();

    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// <see cref="Script"/>'s property, <see cref="ScriptViewModel.Category"/>, has changed.
    /// </summary>
    public event TypeEventHandler<ScriptView>? ScriptChangedCategory;

    /// <summary><see cref="Script"/> was removed.</summary>
    public event TypeEventHandler<ScriptView>? ScriptRemoved;

    public bool AllowEdit { get; private set; }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private ScriptViewModel? Script => (ScriptViewModel?)Content;

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        AllowEdit = !IsReadOnly && newContent is ScriptViewModel newScript && newScript.Type.IsMutable;
        PropertyChanged?.Invoke(this, new(nameof(AllowEdit)));
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e) => ScriptRemoved?.Invoke(this, EventArgs.Empty);

    private void ComboBoxCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Category? old = e.RemovedItems.OfType<Category>().SingleOrDefault();
        if (Script is not null && old is not null && old != Script.Category)
        {
            ScriptChangedCategory?.Invoke(this, EventArgs.Empty);
        }
    }

    private void TextBoxVersionsTextChanged(object sender, TextChangedEventArgs e)
        // Perform validation on every keystore, but don't update the binding.
        => ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).ValidateWithoutUpdate();
}