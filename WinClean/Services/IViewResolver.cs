using System.Windows;

namespace Scover.WinClean.Services;

/// <summary>Resolves views by view model.</summary>
public interface IViewResolver
{
    public FrameworkElement GetView<TViewModel>(TViewModel viewModel);

    public FrameworkElement GetView<TViewModel>() where TViewModel : new();

    public void Register<TViewModel, TView>() where TView : FrameworkElement, new();
}