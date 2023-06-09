using System.Windows;

using Xceed.Wpf.Toolkit;

namespace Scover.WinClean.View;

public static class ButtonsArea
{
    public static readonly DependencyProperty AdditionalButtonsProperty =
        DependencyProperty.RegisterAttached("AdditionalButtons", typeof(object), typeof(ButtonsArea));

    public static readonly DependencyProperty AdditionalContentProperty =
        DependencyProperty.RegisterAttached("AdditionalContent", typeof(object), typeof(ButtonsArea));

    [AttachedPropertyBrowsableForType(typeof(WizardPage))]
    public static object? GetAdditionalButtons(DependencyObject obj)
        => (object?)obj.GetValue(AdditionalButtonsProperty);

    [AttachedPropertyBrowsableForType(typeof(WizardPage))]
    public static object? GetAdditionalContent(DependencyObject obj)
        => (object?)obj.GetValue(AdditionalContentProperty);

    [AttachedPropertyBrowsableForType(typeof(WizardPage))]
    public static void SetAdditionalButtons(DependencyObject obj, object? value)
        => obj.SetValue(AdditionalButtonsProperty, value);

    [AttachedPropertyBrowsableForType(typeof(WizardPage))]
    public static void SetAdditionalContent(DependencyObject obj, object? value)
        => obj.SetValue(AdditionalContentProperty, value);
}