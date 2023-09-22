using System.Diagnostics;

using Scover.WinClean.Model.Metadatas;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.Model.Scripts;

[DebuggerDisplay($"{{{nameof(InvariantName)}}}")]
public sealed class Script : IEquatable<Script?>
{
    public Script(Category category, ScriptCode code, Impact impact, LocalizedString localizedDescription, LocalizedString localizedName, SafetyLevel safetyLevel, string source, ScriptType type, SemVersionRange versions)
        => (Category, Code, Impact, LocalizedDescription, LocalizedName, SafetyLevel, Source, Type, Versions)
            = (category, code, impact, localizedDescription, localizedName, safetyLevel, source, type, versions);

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
    public string Source { get; }
    public ScriptType Type { get; }

    /// <summary>Supported Windows versions range.</summary>
    public SemVersionRange Versions { get; set; }

    public override bool Equals(object? obj) => Equals(obj as Script);

    public bool Equals(Script? other) => other is not null && InvariantName == other.InvariantName;

    public override int GetHashCode() => InvariantName.GetHashCode();
}