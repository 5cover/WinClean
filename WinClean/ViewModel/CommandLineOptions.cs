using System.Diagnostics;

using CommandLine;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.ViewModel.Logging;

using Script = Scover.WinClean.Model.Scripts.Script;

namespace Scover.WinClean.ViewModel;

public sealed class CommandLineOptions
{
    public CommandLineOptions(bool runAllScripts, IEnumerable<string>? scripts, LogLevel verbosity) => (RunAllScripts, Executions, Verbosity) = (runAllScripts, scripts, verbosity);

    [Option("RunAllScripts", SetName = "all scripts", HelpText = "Run all scripts.")]
    public bool RunAllScripts { get; }

    [Option("RunScripts", SetName = "some scripts", Separator = ',',
        HelpText = @"The invariant name of a capability and the invariant name of a script separated by a space, multiple times separated by commas.
Script and capability names are case sensitive and follow ordinal string comparison rules.",
        MetaValue = @"capability1 ""script1"", capability2 ""script2"" ...")]
    public IEnumerable<string>? Executions { get; }

    [Option("Verbosity", HelpText = "The minimum verbosity.")]
    public LogLevel Verbosity { get; }

    public int Execute(IEnumerable<Script> scripts)
    {
        App.Logger.MinLevel = Verbosity;

        // chaud: test error

        // Assert that the mutually exclusive groups were respected.
        Debug.Assert(RunAllScripts ^ Executions is not null);

        Dictionary<Script, ExecutionInfo> executionInfos = new(Executions?.Select(e =>
        {
            var splitted = e.Split(' ', StringSplitOptions.TrimEntries);
            var script = scripts.First(s => s.InvariantName == splitted[1].AsSpan().Trim().Trim('"'));
            return KeyValuePair.Create(script, new ExecutionInfo(script.Code[Capability.FromResourceName(splitted[0])]));
        }) ?? Enumerable.Empty<KeyValuePair<Script, ExecutionInfo>>());

        Logs.StartingExecution.FormatWith(executionInfos.Count).Log(LogLevel.Info);

        foreach ((var script, var executionInfo) in executionInfos)
        {
            Logs.ScriptStartedExecution.FormatWith(script.InvariantName).Log();
            executionInfo.Execute(); // chaud: log result
        }

        Logs.ScriptsExecuted.Log(LogLevel.Info);
        return 0;
    }
}