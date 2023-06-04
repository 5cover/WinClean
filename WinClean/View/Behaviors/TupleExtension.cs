using System.Windows.Markup;

namespace Scover.WinClean.View.Behaviors;

[MarkupExtensionReturnType(typeof((object?, object?)))]
public sealed class TupleExtension : MarkupExtension
{
    public object? Item1 { get; set; }
    public object? Item2 { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => (Item1, Item2);
}
