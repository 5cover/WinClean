using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation.Dialogs;

namespace Scover.WinClean.Presentation.ScriptExecution;

public class CompletedDialog : Dialog
{
    public CompletedDialog(int scriptsCount, TimeSpan elapsed) : base(Button.Restart, Button.OK)
    {
        ExpandedInformation = Resources.UI.CompletedDialog.ExpandedInformation.FormatWith(scriptsCount, elapsed);
        StartExpanded = AppInfo.Settings.DetailsAfterExecution;
        MainInstruction = Resources.UI.CompletedDialog.MainInstruction;
        Content = Resources.UI.CompletedDialog.Content;

        ExpandButtonClicked += (_, _) => AppInfo.Settings.DetailsAfterExecution ^= true;
    }
}