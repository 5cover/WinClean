namespace Scover.WinClean.DataAccess;

public class HungScriptException : Exception
{
    public HungScriptException(string? scriptName) : this(scriptName, null)
    {
    }

    public HungScriptException(string? scriptName, Exception? innerException) : base(Resources.DevException.HungScriptSpecified.FormatWith(scriptName), innerException)
        => ScriptName = scriptName;

    public HungScriptException() : base(Resources.DevException.HungScript)
    {
    }

    public string? ScriptName { get; }
}