using System.Globalization;
using System.Windows.Data;

using Scover.WinClean.Model;

namespace Scover.WinClean.View.Converters;

public sealed class TypedEnumerableDictionaryLookupConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => ((TypedEnumerableDictionary)(value ?? throw new ArgumentNullException(nameof(value))))[(Type)parameter];

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}