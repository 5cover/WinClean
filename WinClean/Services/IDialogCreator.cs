namespace Scover.WinClean.Services;

/// <summary>Creates modal dialog windows from a view model.</summary>
public interface IDialogCreator
{
    public bool? ShowDialog<TViewModel>(TViewModel viewModel);

    public IReadOnlyList<string> ShowOpenFileDialog(ExtensionGroup? extensions = null, string defaultExtension = "", bool multiselect = false, bool readonlyChecked = false);
}