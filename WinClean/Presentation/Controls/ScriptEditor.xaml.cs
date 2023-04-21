using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.Presentation.Converters;

using static Vanara.PInvoke.Ole32.PROPERTYKEY.System;
using static Vanara.PInvoke.Shell32;

namespace Scover.WinClean.Presentation.Controls;

public sealed partial class ScriptEditor
{
    public static readonly DependencyProperty ForbidEditProperty
        = DependencyProperty.Register(nameof(ForbidEdit), typeof(bool), typeof(ScriptEditor));

    public static readonly DependencyProperty IsReadonlyProperty
        = DependencyProperty.Register(nameof(IsReadonly), typeof(bool), typeof(ScriptEditor));

    public static readonly DependencyProperty SelectedProperty
        = DependencyProperty.Register(nameof(Selected), typeof(Script), typeof(ScriptEditor), new(SelectedPropertyChanged));

    public ScriptEditor() => InitializeComponent();

    /// <summary><see cref="Selected"/>'s property, <see cref="IScript.Category"/>, has changed.</summary>
    public event EventHandler? ScriptChangedCategory;

    /// <summary><see cref="Selected"/> was removed.</summary>
    public event EventHandler? ScriptRemoved;

    public bool ForbidEdit
    {
        get => (bool)GetValue(ForbidEditProperty);
        set => SetValue(ForbidEditProperty, value);
    }

    public bool IsReadonly
    {
        get => (bool)GetValue(IsReadonlyProperty);
        set => SetValue(IsReadonlyProperty, value);
    }

    public Script? Selected
    {
        get => (Script?)GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    public static void SelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var scriptEditor = (ScriptEditor)d;
        scriptEditor.ForbidEdit = scriptEditor.IsReadonly || e.NewValue is { } newValue && !((Script)newValue).Type.IsMutable;
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e) => ScriptRemoved?.Invoke(this, EventArgs.Empty);

    private void ComboBoxCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Category? old = e.RemovedItems.OfType<Category>().SingleOrDefault();
        if (Selected is not null && old is not null && old != Selected.Category)
        {
            ScriptChangedCategory?.Invoke(this, EventArgs.Empty);
        }
    }

    public static ImageSource WarningIcon { get; } = SHSTOCKICONID.SIID_WARNING.ToBitmapSource(SHGSI.SHGSI_SMALLICON);
}