using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.Presentation.Windows;

public class CheckableScript : Script
{
    public CheckableScript(Script s)
        : base(s.Category, s.Code, s.Host, s.Impact, s.Versions, s.RecommendationLevel, s.LocalizedDescription, s.LocalizedName, s.Type)
    {
    }

    public bool IsChecked { get; set; }
}
