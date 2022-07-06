using Ookii.Dialogs.Wpf;

using Scover.WinClean.Operational;
using Scover.WinClean.Presentation.Dialogs;

using static Scover.WinClean.Resources.UI.Buttons;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class CompletedDialog : Dialog
{
    public CompletedDialog(int scriptsCount, TimeSpan elapsed)
    {
        ExpandedInformation = Resources.UI.CompletedDialog.ExpandedInformation.FormatWith(scriptsCount, elapsed);
        ExpandedByDefault = App.Settings.DetailsAfterExecution;
        MainInstruction = Resources.UI.CompletedDialog.MainInstruction;
        Content = Resources.UI.CompletedDialog.Content;

        Buttons.Add(new(Close));
        Buttons.Add(new(Restart));

        ExpandButtonClicked += (_, _) => App.Settings.DetailsAfterExecution ^= true;
    }

    /// <returns>A <see cref="DialogResult"/> value based on the button that was clicked.</returns>
    /// <inheritdoc cref="TaskDialog.ShowDialog"/>
    public new DialogResult ShowDialog() => Buttons.IndexOf(base.ShowDialog()) switch
    {
        0 => DialogResult.Close,
        1 => DialogResult.Restart,
        _ => DialogResult.Closed
    };
}