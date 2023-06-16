using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

using Microsoft.Xaml.Behaviors;

using static Vanara.PInvoke.User32;

namespace Scover.WinClean.View;

public class WindowClosing : Behavior<Window>
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(WindowClosing), new(IsEnabledChanged));

    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject.Closing += OnClosing;
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Closing -= OnClosing;
        base.OnDetaching();
    }

    private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (((WindowClosing)d).AssociatedObject is { } window)
        {
            SetCloseMenuItemIsEnabled(new WindowInteropHelper(window).Handle, (bool)e.NewValue);
        }
    }

    private static void SetCloseMenuItemIsEnabled(nint hwnd, bool isEnabled)
    {
        const uint SC_CLOSE = 0xF060;
        _ = EnableMenuItem(GetSystemMenu(hwnd, false), SC_CLOSE, MenuFlags.MF_BYCOMMAND | (isEnabled ? MenuFlags.MF_ENABLED : MenuFlags.MF_GRAYED));
    }

    private void OnClosing(object? sender, CancelEventArgs e) => e.Cancel = !IsEnabled;
}