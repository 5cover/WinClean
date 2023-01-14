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

    public int Execute()
    {
        // Assert that the mutually exclusive groups were respected.
        Debug.Assert(RunAllScripts ^ Scripts is not null);

        var scriptsQuery = App.Scripts;
        if (Scripts is not null)
        {
            scriptsQuery = scriptsQuery.Where(s => Scripts.Contains(s.InvariantName));
        }

        var scripts = scriptsQuery.ToList();

        Logs.StartingExecution.FormatWith(scripts.Count).Log(LogLevel.Info);

        foreach (Script script in scripts)
        {
            Logs.ScriptExecuted.FormatWith(script.InvariantName).Log();
            script.Execute();
        }

        Logs.ScriptsExecuted.Log(LogLevel.Info);
        return 0;
    }
}