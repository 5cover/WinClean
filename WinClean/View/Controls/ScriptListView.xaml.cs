using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View.Controls;

public sealed partial class ScriptListView
{
    public static readonly DependencyProperty DeleteCommandProperty = DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(ScriptListView));
    public static readonly DependencyProperty SelectedScriptProperty = DependencyProperty.Register(nameof(SelectedScript), typeof(ScriptViewModel), typeof(ScriptListView));
    public static readonly DependencyProperty UsageProperty = DependencyProperty.Register(nameof(Usage), typeof(Usage), typeof(ScriptListView));

    public ScriptListView() => InitializeComponent();

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

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

    private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = DeleteCommand.CanExecute(e.Parameter);

    private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DeleteCommand.Execute(e.Parameter);
        // After deletion, focus the datagrid to allow for more deletions.
        _ = this.GetVisualChild<DataGrid>()?.Focus();
    }

    private void DataGridRowRequestBringIntoViewSwallow(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;

    private void DataGridIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var dg = (DataGrid)sender;
        // This is required to make the SelectedItem binding update
        // Indeed, when a data grid is brought into view, the binding doesn't update when it's focused and the selected script doesn't change.
        // So the selected script from before this data grid became visible doesn't change.
        // This is a way to ensure that SelectedScript is always one of scripts displayed.
        if (e.NewValue is bool visible && visible)
        {
            dg.SelectedItem = dg.SelectedItem;
        }
    }
}