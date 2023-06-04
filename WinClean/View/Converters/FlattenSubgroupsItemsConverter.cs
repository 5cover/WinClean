using System.Globalization;
using System.Windows.Data;

namespace Scover.WinClean.View.Converters;

public sealed class FlattenSubgroupsItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((IEnumerable<dynamic>)value).SelectMany<dynamic, object>(g => g.Items);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}