using System.Windows;

using Xceed.Wpf.Toolkit;

namespace Scover.WinClean.View.Behaviors;

public static class Attached
{
    public static readonly DependencyProperty AdditionalButtonsAreaContentProperty =
        DependencyProperty.RegisterAttached("AdditionalButtonsAreaContent", typeof(object), typeof(Attached));

    [AttachedPropertyBrowsableForType(typeof(WizardPage))]
    public static object? GetAdditionalButtonsAreaContent(DependencyObject obj)
        => (object?)obj.GetValue(AdditionalButtonsAreaContentProperty);

    [AttachedPropertyBrowsableForType(typeof(WizardPage))]
    public static void SetAdditionalButtonsAreaContent(DependencyObject obj, object? value)
        => obj.SetValue(AdditionalButtonsAreaContentProperty, value);
}
