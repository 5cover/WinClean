using System.Windows;
using System.Windows.Input;

using Xceed.Wpf.Toolkit;

namespace Scover.WinClean.View.Controls;

public class WizardPageBetter : WizardPage
{
    public static readonly DependencyProperty ConfirmWindowClosingProperty = DependencyProperty.Register(nameof(ConfirmWindowClosing), typeof(bool), typeof(WizardPageBetter));
    public static readonly DependencyProperty EnterCommandProperty = DependencyProperty.Register(nameof(EnterCommand), typeof(ICommand), typeof(WizardPageBetter));
    public static readonly DependencyProperty LeaveCommandProperty = DependencyProperty.Register(nameof(LeaveCommand), typeof(ICommand), typeof(WizardPageBetter));

    static WizardPageBetter() => DefaultStyleKeyProperty.OverrideMetadata(typeof(WizardPageBetter), new FrameworkPropertyMetadata(typeof(WizardPageBetter)));

    public WizardPageBetter()
    {
        Enter += (_, _) => EnterCommand?.Execute(null);
        Leave += (_, _) => LeaveCommand?.Execute(null);
    }

    public bool ConfirmWindowClosing
    {
        get => (bool)GetValue(ConfirmWindowClosingProperty);
        set => SetValue(ConfirmWindowClosingProperty, value);
    }

    public ICommand? EnterCommand
    {
        get => (ICommand)GetValue(EnterCommandProperty);
        set => SetValue(EnterCommandProperty, value);
    }

    public ICommand? LeaveCommand
    {
        get => (ICommand)GetValue(LeaveCommandProperty);
        set => SetValue(LeaveCommandProperty, value);
    }
}