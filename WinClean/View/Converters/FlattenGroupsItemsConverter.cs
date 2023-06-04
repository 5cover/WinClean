using System.Globalization;
using System.Windows.Data;

namespace Scover.WinClean.View.Converters;

public sealed class FlattenGroupsItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((IEnumerable<object>)value).Cast<CollectionViewGroup>().SelectMany(g => g.Items);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}