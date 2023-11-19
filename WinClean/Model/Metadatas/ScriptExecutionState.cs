namespace Scover.WinClean.Model.Metadatas;

public sealed class ScriptExecutionState : Metadata
{
    private ScriptExecutionState(string resourceName) : base(new ResourceTextProvider(Resources.ScriptExecutionStates.ResourceManager, resourceName))
    {
    }
    public static ScriptExecutionState Finished { get; } = new(nameof(Finished));
    public static ScriptExecutionState Paused { get; } = new(nameof(Paused));
    public static ScriptExecutionState Pending { get; } = new(nameof(Pending));
    public static ScriptExecutionState Running { get; } = new(nameof(Running));
    public static ScriptExecutionState Skipped { get; } = new(nameof(Skipped));
}
