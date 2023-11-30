using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Scripts;

public sealed class ScriptAlreadyExistsException : Exception
{
    public ScriptAlreadyExistsException(Script existingScript, Exception? innerException = null)
        : base(ExceptionMessages.ScriptAlreadyExists.FormatWith(existingScript.InvariantName), innerException)
    {
    }
}