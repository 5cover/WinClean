using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Resources;

using Semver;

namespace Scover.WinClean.Model.Serialization;

public sealed class ScriptBuilder
{
    public Category? Category { get; set; }
    public ScriptCode? Code { get; set; }
    public Impact? Impact { get; set; }
    public LocalizedString? LocalizedDescription { get; set; }
    public LocalizedString? LocalizedName { get; set; }
    public SafetyLevel? SafetyLevel { get; set; }

    public SemVersionRange? Versions { get; set; }

    public Script Complete(ScriptType type, string source)
        => Category is null || Code is null || Impact is null || LocalizedDescription is null || LocalizedName is null || SafetyLevel is null || Versions is null
            ? throw new InvalidOperationException(ExceptionMessages.BuilderIncomplete)
            : new Script(Category, Code, Impact, LocalizedDescription, LocalizedName, SafetyLevel, source, type, Versions);
}