using System.Globalization;
using System.Windows.Data;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation.Converters;

public sealed class TypedCollectionDictionaryValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => ((TypedEnumerablesDictionary)(value ?? throw new ArgumentNullException(nameof(value))))[(Type)parameter];

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}