﻿using System.Globalization;

using CommandLine;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.ViewModel.Logging;

using Script = Scover.WinClean.Model.Scripts.Script;

namespace Scover.WinClean.View;

public sealed class CommandLineOptions
{
    public CommandLineOptions(bool listScripts, LogLevel logLevel, IEnumerable<string> runScripts)
    {
        ListScripts = listScripts;
        LogLevel = logLevel;
        RunScripts = runScripts;
    }

    [Option(nameof(ListScripts),
        HelpText = @"List all available scripts.",
        SetName = nameof(ListScripts))]
    public bool ListScripts { get; }

    [Option('v', nameof(LogLevel),
        Default = LogLevel.Info,
        MetaValue = "Verbose/Info/Warning/Error/Critical")]
    public LogLevel LogLevel { get; }

    [Option(nameof(RunScripts), Separator = ',',
                   HelpText = @"Execute a list of scripts.
Script and capability names are case sensitive and follow ordinal string comparison rules.",
        MetaValue = @"""capability1"" ""script1"" ""capability2"" ""script2"" ...",
        SetName = nameof(RunScripts))]
    public IEnumerable<string> RunScripts { get; }

    public int Execute(IEnumerable<Script> scriptsEnumerable)
    {
        Logging.Logger.MinLevel = LogLevel;

        var scripts = scriptsEnumerable.ToList();

        try
        {
            if (ListScripts)
            {
                ExecuteListScripts(scripts);
            }

            if (RunScripts.Any())
            {
                ExecuteRunScripts(scripts);
            }
        }
        // Display argument exceptions instead of letting them go unhandled.
        catch (ConsoleArgumentException e)
        {
            e.Message.Log(LogLevel.Critical);
            return 1;
        }

        return 0;
    }

    private static void ExecuteListScripts(IEnumerable<Script> scriptsEnumerable)
    {
        var scripts = scriptsEnumerable.ToList();

        Console.WriteLine(ConsoleMode.ScriptCount.FormatWith(ConsoleMode.Script.ToQuantity(scripts.Count)));

        foreach (var group in scripts.GroupBy(s => s.Type))
        {
            Console.WriteLine();
            Console.WriteLine($"{group.Key.Name}:");
            Console.WriteLine();
            foreach (var script in group.OrderBy(s => s.Name))
            {
                Console.WriteLine($@"""{script.Name}""");
            }
        }
    }

    private void ExecuteRunScripts(IEnumerable<Script> scripts)
    {
        var executionInfos = (RunScripts.Count() % 2 == 0 ? RunScripts.Chunk(2) : throw new ConsoleArgumentException(ExceptionMessages.CapabilityScriptMismatch, nameof(RunScripts))).Select(e =>
        {
            var (capabilityInvariantName, scriptInvariantName) = (e[0], e[1]);

            var capability = Capability.FromResourceNameOrDefault(capabilityInvariantName)
                ?? throw new ConsoleArgumentException(ExceptionMessages.CapabilityNotFound.FormatWith(capabilityInvariantName), nameof(RunScripts));

            var script = scripts.FirstOrDefault(s => s.InvariantName == scriptInvariantName)
                ?? throw new ConsoleArgumentException(ExceptionMessages.ScriptNotFound.FormatWith(scriptInvariantName), nameof(RunScripts));

            return script.Actions.ContainsKey(capability)
                ? (script, capability)
                : throw new ConsoleArgumentException(ExceptionMessages.ScriptDoesNotHaveCapability.FormatWith(scriptInvariantName, capabilityInvariantName), nameof(RunScripts));
        }).ToList();

        Console.WriteLine(ConsoleMode.StartingExecution.FormatWith(executionInfos.Count));

        foreach (var (script, capability) in executionInfos)
        {
            Console.WriteLine(ConsoleMode.ExecutingScript.FormatWith(script.Name, capability.Name));
            var result = new ExecutionInfo(script.Actions[capability]).Execute();
            Console.WriteLine(ConsoleMode.ScriptExecuted.FormatWith(
                script.Name,
                result.ExitCode,
                Math.Round(result.ExecutionTime.TotalSeconds).Seconds().ToString("g", CultureInfo.CurrentCulture),
                result.Succeeded));
        }
    }

    private sealed class ConsoleArgumentException : ArgumentException
    {
        public ConsoleArgumentException(string message, string argumentName) : base(message, argumentName)
        {
        }
    }
}