using System.Windows;
using System.Windows.Controls;
using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.Presentation.Controls;

public sealed partial class ScriptEditor
{
    public static readonly DependencyProperty ForbidEditProperty
        = DependencyProperty.Register(nameof(ForbidEdit), typeof(bool), typeof(ScriptEditor));

    public static readonly DependencyProperty SelectedProperty
        = DependencyProperty.Register(nameof(Selected), typeof(Script), typeof(ScriptEditor), new(SelectedPropertyChanged));

    public ScriptEditor() => InitializeComponent();

    /// <summary><see cref="Selected"/>'s property, <see cref="Script.Category"/>, has changed.</summary>
    public event EventHandler? ScriptChangedCategory;

    /// <summary><see cref="Selected"/> was removed.</summary>
    public event EventHandler? ScriptRemoved;

    public bool ForbidEdit
    {
        get => (bool)GetValue(ForbidEditProperty);
        set => SetValue(ForbidEditProperty, value);
    }

    public Script? Selected
    {
        get => (Script?)GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    public static void SelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ScriptEditor)d).ForbidEdit = (e.NewValue as Script)?.Type == ScriptType.Default;

    private void ButtonDelete_Click(object sender, RoutedEventArgs e) => ScriptRemoved?.Invoke(this, EventArgs.Empty);

    private void ComboBoxCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Category? old = e.RemovedItems.OfType<Category>().SingleOrDefault();
        if (Selected is not null && old is not null && old != Selected.Category)
        {
            ScriptChangedCategory?.Invoke(this, EventArgs.Empty);
        }
    }
}