using System.Diagnostics;

using CommandLine;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.Presentation.Logging;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Presentation;

public sealed class CommandLineOptions
{
    [Option("runAllScripts", SetName = "A", HelpText = "Run all scripts.")]
    public bool RunAllScripts { get; set; }

    [Option("runScripts", SetName = "B", HelpText = "The invariant names of the scripts to run, quoted and separated by spaces.", MetaValue = "\"script1\" \"script2\" ...")]
    public IEnumerable<string>? Scripts { get; set; }

    public int Execute(IEnumerable<Script> scripts)
    {
        // Assert that the mutually exclusive groups were respected.
        Debug.Assert(RunAllScripts ^ Scripts is not null);

        List<Script> scriptsToRun = (Scripts is null ? scripts : scripts.Where(s => Scripts.Contains(s.InvariantName))).ToList();

        Logs.StartingExecution.FormatWith(scriptsToRun.Count).Log(LogLevel.Info);

        foreach (Script script in scriptsToRun)
        {
            Logs.ScriptExecuted.FormatWith(script.InvariantName).Log();
            _ = script.Execute();
        }

        Logs.ScriptsExecuted.Log(LogLevel.Info);
        return 0;
    }
}