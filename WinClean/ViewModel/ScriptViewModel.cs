using System.Diagnostics;

using CommunityToolkit.Mvvm.ComponentModel;

using Optional;
using Optional.Collections;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Scripts;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;
using Scover.WinClean.ViewModel.Logging;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.ViewModel;

public sealed class ScriptViewModel : ObservableObject, IEquatable<ScriptViewModel?>, IEquatable<Script?>
{
    private KeyValuePair<Capability, ScriptActionViewModel> _selectedAction;

    public ScriptViewModel(Script model)
    {
        Model = model;
        Actions = model.Actions.ToDictionary(kv => kv.Key, kv => CreateScriptActionViewModel(kv.Value));
        SelectedAction = Actions.First();
        EffectiveCapability = new(() => DetectCapability(Settings.ScriptDetectionTimeout), ct =>
        {
            using CancellationTokenSource cts = new();
            using var regCt = ct.Register(cts.Cancel);
            cts.CancelAfter(Settings.ScriptDetectionTimeout);
            return DetectCapabilityAsync(cts.Token);
        });
    }

    private ScriptActionViewModel CreateScriptActionViewModel(ScriptAction action)
    {
        ScriptActionViewModel viewModel = new(action);
        viewModel.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Actions));
        return viewModel;
    }

    public IReadOnlyDictionary<Capability, ScriptActionViewModel> Actions { get; }

    public Category Category
    {
        get => Model.Category;
        set
        {
            Model.Category = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => Model.LocalizedDescription[CurrentUICulture];
        set => Model.LocalizedDescription[CurrentUICulture] = value;
    }

    public Cached<Capability?> EffectiveCapability { get; }

    public Option<TimeSpan> ExecutionTime
    {
        get => Settings.ScriptExecutionTimes.TryGetValue(InvariantName, out var time) ? time.Some() : time.None();
        set
        {
            value.Match(time => Settings.ScriptExecutionTimes.SetOrAdd(InvariantName, time),
                        () => Settings.ScriptExecutionTimes.Remove(InvariantName));
            OnPropertyChanged();
        }
    }

    public Impact Impact
    {
        get => Model.Impact;
        set
        {
            Model.Impact = value;
            OnPropertyChanged();
        }
    }

    public string InvariantName => Model.InvariantName;

    public string Name
    {
        get => Model.Name;
        set
        {
            Model.Name = value;
            OnPropertyChanged();
        }
    }

    public SafetyLevel SafetyLevel
    {
        get => Model.SafetyLevel;
        set
        {
            Model.SafetyLevel = value;
            OnPropertyChanged();
        }
    }

    public KeyValuePair<Capability, ScriptActionViewModel> SelectedAction
    {
        get => _selectedAction;
        set
        {
            _selectedAction = value;
            OnPropertyChanged();
        }
    }

    public ScriptSelection Selection { get; } = new();
    public ScriptType Type => Model.Type;
    public IReadOnlyCollection<Usage> Usages => Usage.GetUsages(Model).ToList();

    public SemVersionRange Versions
    {
        get => Model.Versions;
        set
        {
            Model.Versions = value;
            OnPropertyChanged();
        }
    }

    internal Script Model { get; }
    private static ISettings Settings => ServiceProvider.Get<ISettings>();

    private Option<ScriptAction> DetectAction => Actions.GetValueOrNone(Capability.Detect).Map(a => a.Model);

    public override bool Equals(object? obj) => Equals(obj as ScriptViewModel);

    public bool Equals(ScriptViewModel? other) => other is not null && Model.Equals(other.Model);

    public bool Equals(Script? other) => Model.Equals(other);

    public override int GetHashCode() => Model.GetHashCode();

    public Option<ExecutionInfoViewModel> TryCreateExecutionInfo() =>
        Selection.DesiredCapability is { } desiredCapability // A capability has be choosen
        && Actions.TryGetValue(desiredCapability, out var action) // The capability exists
        ? new ExecutionInfoViewModel(this, desiredCapability, action.Model).Some()
        : Option.None<ExecutionInfoViewModel>();

    private Capability? DetectCapability(TimeSpan timeout) => DetectAction.Match(detect =>
    {
        using HostStartInfo startInfo = detect.CreateHostStartInfo();
        using Process process = StartDetection(startInfo);

        if (process.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
        {
            var effectiveCapability = Capability.FromInteger(process.ExitCode);
            LogCapabilityDetectionDone(effectiveCapability);
            return effectiveCapability;
        }

        // Detection took too long: kill process.
        process.KillTree();
        LogCapabilityDetectionCanceled();

        return null;
    }, () => null);

    private Task<Capability?> DetectCapabilityAsync(CancellationToken cancellationToken) => DetectAction.Match(async detect =>
    {

        using HostStartInfo startInfo = detect.CreateHostStartInfo();
        using var process = StartDetection(startInfo);

        using var reg = cancellationToken.Register(() =>
        {
            process.KillTree();
            LogCapabilityDetectionCanceled();
        });

        await process.WaitForExitAsync(cancellationToken);

        var effectiveCapability = Capability.FromInteger(process.ExitCode);

        if (!cancellationToken.IsCancellationRequested)
        {
            LogCapabilityDetectionDone(effectiveCapability);
        }

        return effectiveCapability;

    }, () => Task.FromResult<Capability?>(null));

    private Process StartDetection(HostStartInfo startInfo)
    {
        Logs.CapabilityDetectionStarted.FormatWith(InvariantName).Log();
        return Process.Start(new ProcessStartInfo()
        {
            FileName = startInfo.Filename,
            Arguments = startInfo.Arguments,
            CreateNoWindow = true,
        }).NotNull();
    }

    private void LogCapabilityDetectionCanceled() => Logs.CapabilityDetectionCanceled.FormatWith(InvariantName).Log(LogLevel.Error);
    private void LogCapabilityDetectionDone(Capability? result) => Logs.CapabilityDetectionDone.FormatWith(InvariantName, result?.InvariantName).Log();
}