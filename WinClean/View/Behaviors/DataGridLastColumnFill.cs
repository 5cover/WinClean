using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

namespace Scover.WinClean.View.Behaviors;

public sealed class DataGridLastColumnFill : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += ResizeColumns;
        AssociatedObject.SizeChanged += ResizeColumns;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= ResizeColumns;
        AssociatedObject.SizeChanged -= ResizeColumns;
        base.OnDetaching();
    }

    private void ResizeColumns(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject.Columns.LastOrDefault() is { } lastColumn)
        {
            lastColumn.Width = new(1, DataGridLengthUnitType.Star);
        }
    }
}