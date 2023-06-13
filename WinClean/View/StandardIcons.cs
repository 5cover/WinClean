using System.Windows.Media;

using static Vanara.PInvoke.Shell32;

namespace Scover.WinClean.View;

public static class StandardIcons
{
    public static ImageSource Error { get; } = SHSTOCKICONID.SIID_ERROR.ToBitmapSource(SHGSI.SHGSI_SMALLICON);
    public static ImageSource Warning { get; } = SHSTOCKICONID.SIID_WARNING.ToBitmapSource(SHGSI.SHGSI_SMALLICON);
}