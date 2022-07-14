using System.Globalization;

namespace Scover.WinClean.BusinessLogic.Scripts;

public abstract class LocalizableScriptMetadata : IUserVisible
{
    protected LocalizableScriptMetadata(LocalizedString names, LocalizedString descriptions)
    {
        Names = names;
        Descriptions = descriptions;
    }

    public string Description => Descriptions.Get(CultureInfo.CurrentUICulture);
    public string InvariantName => Names.Get(CultureInfo.InvariantCulture);
    public string Name => Names.Get(CultureInfo.CurrentUICulture);
    protected LocalizedString Descriptions { get; }
    protected LocalizedString Names { get; }
}