namespace Scover.WinClean.BusinessLogic;

public class ScriptExecutionProgressChangedEventArgs : EventArgs
{
    public ScriptExecutionProgressChangedEventArgs(int scriptIndex) => ScriptIndex = scriptIndex;

    /// <summary>IThe index of the last executed script.</summary>
    public int ScriptIndex { get; }
}