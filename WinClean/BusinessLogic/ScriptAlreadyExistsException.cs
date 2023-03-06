using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

public sealed class ScriptAlreadyExistsException : Exception
{
    public ScriptAlreadyExistsException(Script existingScript, Exception? innerException = null)
        : base($"The script '{existingScript.InvariantName}' already exists", innerException)
        => ExistingScript = existingScript;

    public Script ExistingScript { get; }
}
