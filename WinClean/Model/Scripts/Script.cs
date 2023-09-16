using Scover.WinClean.Model.Metadatas;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.Model.Scripts;

public sealed class Script : IEquatable<Script?>
{
    public Script(Category category, Impact impact, SemVersionRange versions, SafetyLevel safetyLevel, LocalizedString localizedDescription, LocalizedString localizedName, ScriptType type, ScriptCode code)
        => (Category, Impact, Versions, SafetyLevel, LocalizedDescription, LocalizedName, Type, Code)
            = (category, impact, versions, safetyLevel, localizedDescription, localizedName, type, code);

    public Category Category { get; set; }
    public ScriptCode Code { get; }
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
    public ScriptType Type { get; }

    /// <summary>Supported Windows versions range.</summary>
    public SemVersionRange Versions { get; set; }

    public override bool Equals(object? obj) => Equals(obj as Script);

    public bool Equals(Script? other) => other is not null && InvariantName == other.InvariantName;

    public override int GetHashCode() => InvariantName.GetHashCode();
}