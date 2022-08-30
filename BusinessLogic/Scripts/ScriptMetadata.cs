using System.Globalization;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>Common properties for script metadata objects.</summary>
public abstract class ScriptMetadata : IScriptData
{
    protected ScriptMetadata(LocalizedString names, LocalizedString descriptions)
    {
        Names = names;
        Descriptions = descriptions;
    }

    public string Description => Descriptions.Get(CultureInfo.CurrentUICulture);
    public string InvariantName => Names.Get(CultureInfo.InvariantCulture);
    public string Name => Names.Get(CultureInfo.CurrentUICulture);
    private LocalizedString Descriptions { get; }
    private LocalizedString Names { get; }
}