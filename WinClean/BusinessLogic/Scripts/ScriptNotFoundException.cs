namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class ScriptNotFoundException : Exception
{
    public ScriptNotFoundException(string source, Exception? innerException = null)
        : base($"The script at '{source}' was not found", innerException)
        => ScriptSource = source;

    public string ScriptSource { get; }
}