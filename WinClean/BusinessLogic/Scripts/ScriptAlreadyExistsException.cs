namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class ScriptAlreadyExistsException : Exception
{
    public ScriptAlreadyExistsException(Script existingScript, Exception innerException)
        : base($"The script '{existingScript.InvariantName}' already exists", innerException)
        => ExistingScript = existingScript;

    public Script ExistingScript { get; }
}