using System.Windows;

namespace Scover.WinClean.Services;

public sealed class ViewFactory : IViewFactory
{
    private readonly Dictionary<Type, Type> _viewsByViewModels = new();

    public FrameworkElement MakeView<TViewModel>(TViewModel viewModel)
    {
        var view = (FrameworkElement)Activator.CreateInstance(_viewsByViewModels[typeof(TViewModel)]).NotNull();
        view.DataContext = viewModel;
        return view;
    }

    public void Register<TViewModel, TView>() where TView : FrameworkElement, new()
        => _viewsByViewModels.Add(typeof(TViewModel), typeof(TView));
}