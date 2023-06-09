using Scover.WinClean.ViewModel;

namespace Scover.WinClean.View.Controls;

public sealed partial class ExecutionInfoView
{
    public ExecutionInfoView() => InitializeComponent();

    private void TextEditor_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e) => ((ExecutionInfoViewModel)Content).NotifyScroll.Execute(e);
}