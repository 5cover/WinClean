namespace Scover.WinClean.BusinessLogic;

public sealed class ScriptExecutionProgressChangedEventArgs : EventArgs
{
    public ScriptExecutionProgressChangedEventArgs(int scriptIndex) => ScriptIndex = scriptIndex;

    /// <summary>Gets the index of the last executed script.</summary>
    public int ScriptIndex { get; }
}