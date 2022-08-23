using Scover.WinClean.DataAccess;

using System.Management.Automation;

namespace Scover.WinClean.BusinessLogic.Scripts.Hosts;

public class PowerShell : IHost
{
    private PowerShell()
    {
    }

    public static PowerShell Instance { get; } = new();

    public string Description { get; } = Resources.Host.PowerShellDescription;
    public string InvariantName { get; } = "PowerShell";
    public string Name { get; } = "PowerShell";
    public ExtensionGroup SupportedExtensions { get; } = new(".ps1");

    public void ExecuteCode(string code, string scriptName, TimeSpan timeout, HungScriptCallback keepRunningOrKill, CancellationToken cancellationToken)
    {
        var ps = System.Management.Automation.PowerShell.Create();
        ps.AddScript(code);

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
            // Abort the execution by stopping PowerShell.
            ps.Stop();
        }
    }
}