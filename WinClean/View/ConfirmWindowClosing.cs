using System.ComponentModel;
using System.Windows;

using Microsoft.Xaml.Behaviors;

using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View;

public sealed class ConfirmWindowClosing : Behavior<Window>
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(ConfirmWindowClosing));

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

    private void OnClosing(object? sender, CancelEventArgs e) => e.Cancel = IsEnabled && !DialogFactory.ShowConfirmation(DialogFactory.MakeConfirmAbortOperation);
}