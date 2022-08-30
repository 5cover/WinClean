using System.Management.Automation;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts.Hosts;

/// <summary>PowerShell script host.</summary>
public class PowerShell : IHost
{
    private PowerShell()
    {
    }

    /// <summary>Gets the instance of this singleton.</summary>
    public static PowerShell Instance { get; } = new();

    public string Description { get; } = Resources.Host.PowerShellDescription;
    public string InvariantName { get; } = "PowerShell";
    public string Name { get; } = "PowerShell";
    public ExtensionGroup SupportedExtensions { get; } = new(".ps1");

    public void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningOrKill, CancellationToken cancellationToken)
    {
        var ps = System.Management.Automation.PowerShell.Create();
        ps.AddScript(code);

        using var registration = cancellationToken.Register(ps.Stop);

        try
        {
            while (!ps.InvokeAsync().Wait(Convert.ToInt32(timeout.TotalMilliseconds), cancellationToken))
            {
                if (!keepRunningOrKill(scriptName))
                {
                    ps.Stop();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // already stopped ps with the registration.
        }

        registration.Unregister();
    }
}