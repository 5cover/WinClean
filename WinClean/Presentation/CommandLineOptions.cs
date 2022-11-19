using System.Diagnostics;

using CommandLine;

using Scover.WinClean.BusinessLogic;
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
        Debug.Assert(RunAllScripts ^ Scripts is not null);

        using ScriptExecutor executor = new();

        var scriptsQuery = App.ScriptCollections.Values.SelectMany(x => x);
        if (Scripts is not null)
        {
            scriptsQuery = scriptsQuery.Where(s => Scripts.Contains(s.InvariantName));
        }

        var scripts = scriptsQuery.ToList();

        Logs.StartingExecutionOfScripts.FormatWith(scripts.Count).Log(LogLevel.Info);

        executor.ExecuteScriptsAsync(scripts, scriptName =>
        {
            Logs.HungScript.FormatWith(scriptName, AppInfo.Settings.ScriptTimeout).Log(LogLevel.Warning);
            Logs.HungScriptAborted.FormatWith(scriptName).Log(LogLevel.Info);
            return false;
        }).Wait();

        Logs.ScriptsExecuted.Log(LogLevel.Info);
        return 0;
    }
}