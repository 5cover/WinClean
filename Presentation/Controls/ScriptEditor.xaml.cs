using System.Windows;
using System.Windows.Controls;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.Properties;

namespace Scover.WinClean.Presentation.Controls;

public partial class ScriptEditor
{
    public static readonly DependencyProperty SelectedProperty
        = DependencyProperty.Register(nameof(Selected), typeof(Script), typeof(ScriptEditor));

    public ScriptEditor() => InitializeComponent();

    /// <summary><see cref="Selected"/>'s property, <see cref="Script.Category"/>, has changed.</summary>
    public event EventHandler? ScriptChangedCategory;

    public bool ForbidEdit => Selected?.IsDefault ?? true || !AppInfo.Settings.AllowCustomScriptEdit;

    public Script? Selected
    {
        get => (Script?)GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    private void ComboBoxCategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Category? old = e.RemovedItems.OfType<Category>().SingleOrDefault();
        if (Selected is not null && old is not null && old != Selected.Category)
        {
            ScriptChangedCategory?.Invoke(this, EventArgs.Empty);
        }
    }
}