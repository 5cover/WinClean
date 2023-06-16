using System.Windows;

using Microsoft.Win32;

namespace Scover.WinClean.Services;

public sealed class DialogCreator : IDialogCreator
{
    private static Window ActiveWindow => Application.Current.Windows.OfType<Window>().Single(wnd => wnd.IsActive);

    public bool? ShowDialog<TViewModel>(TViewModel viewModel)
    {
        var view = (Window)ServiceProvider.Get<IViewResolver>().GetView(viewModel);
        view.Owner = ActiveWindow;
        view.DataContext = viewModel;
        return view.ShowDialog();
    }

    public IReadOnlyList<string> ShowOpenFileDialog(string filter, string defaultExtension = "", bool multiselect = false, bool readonlyChecked = false)
    {
        OpenFileDialog ofd = new()
        {
            Filter = filter,
            DefaultExt = defaultExtension,
            Multiselect = multiselect,
            ReadOnlyChecked = readonlyChecked
        };
        return !ofd.ShowDialog(ActiveWindow) ?? true ? Array.Empty<string>() : ofd.FileNames;
    }
}