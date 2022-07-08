using Scover.WinClean.BusinessLogic;
using Scover.WinClean.BusinessLogic.ScriptExecution;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;
using Scover.WinClean.Presentation.ScriptExecution;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Scover.WinClean.Presentation.Controls;

public partial class ScriptEditor : UserControl
{
    public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register(nameof(Owner), typeof(Selector), typeof(ScriptEditor));

    public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(nameof(Selected), typeof(Script), typeof(ScriptEditor));

    public ScriptEditor() => InitializeComponent();

    /// <summary><see cref="Selected"/>'s property, <see cref="Script.Category"/>, has changed.</summary>
    public event EventHandler<ScriptChangedCategoryEventArgs>? ScriptChangedCategory;

    /// <summary>Get or sets the <see cref="Selector"/> that <see cref="Selected"/> is in.</summary>
    /// <remarks>If <see cref="Selected"/> is in an <see cref="Selector"/>, must be set to it.</remarks>
    public Selector? Owner
    {
        get => (ListBox?)GetValue(OwnerProperty);
        set => SetValue(OwnerProperty, value);
    }

    public Script? Selected
    {
        get => (Script?)GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }

    private void ButtonDelete_Click(object sender, RoutedEventArgs e)
    {
        if (Selected is not null && YesNoDialog.ScriptDeletion.ShowDialog() == DialogResult.Yes)
        {
            File.Delete(AppDirectory.ScriptsDir.Join(Selected.Filename));
            if (Owner is not null)
            {
                Owner.ItemsSource = Owner.ItemsSource.OfType<Script>().Where(s => s != Selected);
            }
        }
    }

    private void ButtonExecute_Click(object sender, RoutedEventArgs e)
    {
        if (Selected is not null)
        {
            new ScriptExecutionWizard(Selected).ExecuteNoUI();
        }
    }

    private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Category? old = e.RemovedItems.OfType<Category>().SingleOrDefault();
        if (Selected is not null && old is not null && old != Selected.Category)
        {
            ScriptChangedCategory?.Invoke(this, new(Selected, old, Selected.Category));
        }
    }

    private void ComboBoxHost_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Selected is not null)
        {
            Selected.Host = ScriptHostFactory.FromDisplayName((string)((ComboBox)sender).SelectedItem);
        }
    }
}