namespace Scover.WinClean.BusinessLogic.Scripts;

public class ScriptAlreadyExistsException : Exception
{
    public ScriptAlreadyExistsException(Script existingScript, Exception? innerException = null)
        : base(Resources.DevException.ScriptAlreadyExists.FormatWith(existingScript.InvariantName), innerException)
        => ExistingScript = existingScript;

    public Script ExistingScript { get; }
}