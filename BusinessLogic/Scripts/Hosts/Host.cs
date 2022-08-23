using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts.Hosts;

/// <summary>Represents a program that accepts a file in it's command-line arguments.</summary>
public interface IHost : IScriptData
{
    /// <summary>Gets the file extensions of the scripts the script host program can run, in the order of preference.</summary>
    ExtensionGroup SupportedExtensions { get; }

    /// <summary>Executes the specified code.</summary>
    /// <param name="code">The code to execute.</param>
    /// <param name="scriptName">The name of the script to execute.</param>
    /// <param name="timeout">The script timeout.</param>
    /// <param name="keepRunningOrKill">Chooses whether a hung script should be allowed to keep running or be killed.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that aborts the execution by killing the host process when the cancelled. Can be <see
    /// langword="null"/> to disable cancellation.
    /// </param>
    void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningOrKill, CancellationToken cancellationToken);
}