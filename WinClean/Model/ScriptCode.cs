using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model;

public sealed class ScriptCode : IDictionary<Capability, ScriptAction>
{
    private readonly Dictionary<Capability, ScriptAction> _actions;

    public ScriptCode(Dictionary<Capability, ScriptAction> actions) => _actions = actions;

    public int Count => _actions.Count;
    public ICollection<Capability> Keys => _actions.Keys;
    public ICollection<ScriptAction> Values => _actions.Values;
    bool ICollection<KeyValuePair<Capability, ScriptAction>>.IsReadOnly => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).IsReadOnly;
    public ScriptAction this[Capability key] { get => _actions[key]; set => _actions[key] = value; }

    public void Add(Capability key, ScriptAction value) => _actions.Add(key, value);

    public bool ContainsKey(Capability key) => _actions.ContainsKey(key);

    public Capability? DetectCapability(TimeSpan timeout)
    {
        if (!TryGetValue(Capability.Detect, out ScriptAction? detect))
        {
            return null;
        }

        using HostStartInfo startInfo = detect.CreateHostStartInfo();
        using Process process = StartProcess(startInfo);

        if (process.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
        {
            return Capability.FromInteger(process.ExitCode);
        }

        // Detection took too long : kill process.
        process.Kill(true);
        return null;
    }

    public async Task<Capability?> DetectCapabilityAsync(CancellationToken cancellationToken)
    {
        if (!TryGetValue(Capability.Detect, out var detect))
        {
            return null;
        }

        using HostStartInfo startInfo = detect.CreateHostStartInfo();
        using var process = StartProcess(startInfo);

        using var reg = cancellationToken.Register(() => process.Kill(true));

        await process.WaitForExitAsync(cancellationToken);

        return Capability.FromInteger(process.ExitCode);
    }

    public IEnumerator<KeyValuePair<Capability, ScriptAction>> GetEnumerator() => ((IEnumerable<KeyValuePair<Capability, ScriptAction>>)_actions).GetEnumerator();

    public bool Remove(Capability key) => _actions.Remove(key);

    public bool TryGetValue(Capability key, [MaybeNullWhen(false)] out ScriptAction value) => _actions.TryGetValue(key, out value);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.Add(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Add(item);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.Clear() => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Clear();

    bool ICollection<KeyValuePair<Capability, ScriptAction>>.Contains(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Contains(item);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.CopyTo(KeyValuePair<Capability, ScriptAction>[] array, int arrayIndex) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_actions).GetEnumerator();

    bool ICollection<KeyValuePair<Capability, ScriptAction>>.Remove(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Remove(item);

    private static Process StartProcess(HostStartInfo startInfo) => Process.Start(new ProcessStartInfo()
    {
        FileName = startInfo.Filename,
        Arguments = startInfo.Arguments,
        CreateNoWindow = true,
    }).NotNull();
}