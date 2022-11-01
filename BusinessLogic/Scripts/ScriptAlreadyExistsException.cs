namespace Scover.WinClean.BusinessLogic.Scripts;

public sealed class ScriptAlreadyExistsException : Exception
{
    public ScriptAlreadyExistsException(Script existingScript, Exception innerException)
        : base(Resources.DevException.ScriptAlreadyExists.FormatWith(existingScript.InvariantName), innerException)
        => ExistingScript = existingScript;

    public Script ExistingScript { get; }
}