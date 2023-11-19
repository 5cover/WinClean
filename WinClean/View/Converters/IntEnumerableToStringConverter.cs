using System.Globalization;
using System.Windows.Data;

namespace Scover.WinClean.View.Converters;

public sealed class IntEnumerableToStringConverter : IValueConverter
{
    private const char SeparatorChar = ' ';

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => string.Join(SeparatorChar, (IEnumerable<int>)value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}