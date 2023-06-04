using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Scover.WinClean.View.Behaviors;

[ContentProperty(nameof(Binding))]
public sealed class ConverterBindableParameterExtension : MarkupExtension
{
    public Binding? Binding { get; set; }
    public BindingMode Mode { get; set; }
    public IValueConverter? Converter { get; set; }
    public Binding? ConverterParameter { get; set; }

    public ConverterBindableParameterExtension()
    { }

    public ConverterBindableParameterExtension(string path) => Binding = new Binding(path);

    public ConverterBindableParameterExtension(Binding binding) => Binding = binding;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var multiBinding = new MultiBinding();

        if (Binding is not null)
        {
            Binding.Mode = Mode;
            multiBinding.Bindings.Add(Binding);
        }

        if (ConverterParameter is not null)
        {
            ConverterParameter.Mode = BindingMode.OneWay;
            multiBinding.Bindings.Add(ConverterParameter);
        }

        if (Converter is not null)
        {
            multiBinding.Converter = new MultiValueConverterAdapter
            {
                Converter = Converter
            };
        }

        return multiBinding.ProvideValue(serviceProvider);
    }

    [ContentProperty(nameof(Converter))]
    private sealed class MultiValueConverterAdapter : IMultiValueConverter
    {
        public IValueConverter? Converter { get; set; }

        private object? lastParameter;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                lastParameter = values[1];
            }
            return values.Length is 1 or 2
                ? Converter?.Convert(values[0], targetType, lastParameter, culture) ?? values[0]
                : throw new NotSupportedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => new[] { Converter?.ConvertBack(value, targetTypes[0], lastParameter, culture) ?? value };
    }
}