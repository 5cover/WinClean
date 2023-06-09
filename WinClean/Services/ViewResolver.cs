using System.Windows;

namespace Scover.WinClean.Services;

public sealed class ViewResolver : IViewResolver
{
    private readonly Dictionary<Type, Type> _viewsByViewModels = new();

    public FrameworkElement GetView<TViewModel>(TViewModel viewModel)
    {
        var view = (FrameworkElement)Activator.CreateInstance(_viewsByViewModels[typeof(TViewModel)]).NotNull();
        view.DataContext = viewModel;
        return view;
    }

    public FrameworkElement GetView<TViewModel>() where TViewModel : new() => GetView(new TViewModel());

    public void Register<TViewModel, TView>() where TView : FrameworkElement, new()
                => _viewsByViewModels[typeof(TViewModel)] = typeof(TView);
}