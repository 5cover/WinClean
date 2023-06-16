using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Scover.WinClean.View.Converters;

public sealed class DictionaryLookupConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object parameter, CultureInfo culture)
    {
        Debug.Assert(values.Length == 2);
        var dic = values[0] as IDictionary;
        var key = values[1];
        return key is null ? null : dic?[key];
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}