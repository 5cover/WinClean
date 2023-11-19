using System.Diagnostics;

using Scover.WinClean.Model.Metadatas;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.Model.Scripts;

[DebuggerDisplay($"{{{nameof(InvariantName)}}}")]
public sealed class Script
{
    public Script(IReadOnlyDictionary<Capability, ScriptAction> actions, Category category, Impact impact, LocalizedString localizedDescription, LocalizedString localizedName, SafetyLevel safetyLevel, string source, ScriptType type, SemVersionRange versions)
        => (Actions, Category, Impact, LocalizedDescription, LocalizedName, SafetyLevel, Source, Type, Versions)
            = (actions, category, impact, localizedDescription, localizedName, safetyLevel, source, type, versions);

    public IReadOnlyDictionary<Capability, ScriptAction> Actions { get; }
    public Category Category { get; set; }
    public Impact Impact { get; set; }
    public string InvariantName => LocalizedName[InvariantCulture];
    public LocalizedString LocalizedDescription { get; init; }
    public LocalizedString LocalizedName { get; init; }

    public string Name
    {
        get => LocalizedName[CurrentUICulture];
        set => LocalizedName[CurrentUICulture] = value;
    }

    public SafetyLevel SafetyLevel { get; set; }
    public string Source { get; }
    public ScriptType Type { get; }

    /// <summary>Supported Windows versions range.</summary>
    public SemVersionRange Versions { get; set; }
}