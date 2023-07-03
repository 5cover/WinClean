using System.Windows;

namespace Scover.WinClean.Services;

/// <summary>Creates views by view model.</summary>
/// <remarks>
/// Used for instantiating views from view models without refering to a specific view type.
/// </remarks>
public interface IViewFactory
{
    /// <summary>
    /// Creates a view of a type previously registered using <see cref="Register{TViewModel, TView}"/>.
    /// </summary>
    /// <param name="viewModel">
    /// The view model to assign to the <see cref="FrameworkElement.DataContext"/> property.
    /// </param>
    /// <returns>A new view object.</returns>
    /// <exception cref="KeyNotFoundException">
    /// The view for <typeparamref name="TViewModel"/> hasn't been registered.
    /// </exception>
    public FrameworkElement MakeView<TViewModel>(TViewModel viewModel);

    /// <remarks>Each view model can only be registered once.</remarks>
    /// <exception cref="ArgumentException">This view model has already been registered.</exception>
    public void Register<TViewModel, TView>() where TView : FrameworkElement, new();
}