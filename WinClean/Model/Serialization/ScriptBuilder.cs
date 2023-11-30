using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Resources;

using Semver;

namespace Scover.WinClean.Model.Serialization;

public sealed class ScriptBuilder
{
    public IReadOnlyDictionary<Capability, ScriptAction>? Actions { get; init; }
    public Category? Category { get; init; }
    public Impact? Impact { get; init; }
    public LocalizedString? LocalizedDescription { get; init; }
    public LocalizedString? LocalizedName { get; init; }
    public SafetyLevel? SafetyLevel { get; init; }

    public SemVersionRange? Versions { get; init; }

    public Script Complete(ScriptType type, string source)
        => Category is null || Actions is null || Impact is null || LocalizedDescription is null || LocalizedName is null || SafetyLevel is null || Versions is null
            ? throw new InvalidOperationException(ExceptionMessages.BuilderIncomplete)
            : new Script(Actions, Category, Impact, LocalizedDescription, LocalizedName, SafetyLevel, source, type, Versions);
}