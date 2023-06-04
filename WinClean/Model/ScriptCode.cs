using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model;

public sealed class ScriptCode : IDictionary<Capability, ScriptAction>
{
    private readonly Dictionary<Capability, ScriptAction> _actions;

    public ScriptCode(Dictionary<Capability, ScriptAction> actions) => _actions = actions;

    public ICollection<Capability> Keys => _actions.Keys;
    public ICollection<ScriptAction> Values => _actions.Values;
    public int Count => _actions.Count;
    bool ICollection<KeyValuePair<Capability, ScriptAction>>.IsReadOnly => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).IsReadOnly;
    public ScriptAction this[Capability key] { get => _actions[key]; set => _actions[key] = value; }

    public void Add(Capability key, ScriptAction value) => _actions.Add(key, value);

    public bool ContainsKey(Capability key) => _actions.ContainsKey(key);

    public IEnumerator<KeyValuePair<Capability, ScriptAction>> GetEnumerator() => ((IEnumerable<KeyValuePair<Capability, ScriptAction>>)_actions).GetEnumerator();

    public bool Remove(Capability key) => _actions.Remove(key);

    public bool TryGetValue(Capability key, [MaybeNullWhen(false)] out ScriptAction value) => _actions.TryGetValue(key, out value);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.Add(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Add(item);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.Clear() => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Clear();

    bool ICollection<KeyValuePair<Capability, ScriptAction>>.Contains(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Contains(item);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.CopyTo(KeyValuePair<Capability, ScriptAction>[] array, int arrayIndex) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_actions).GetEnumerator();

    bool ICollection<KeyValuePair<Capability, ScriptAction>>.Remove(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_actions).Remove(item);

    public async Task<Capability?> DetectCapabilityAsync(CancellationToken cancellationToken)
    {
        if (!TryGetValue(Capability.Detect, out var detect))
        {
            return null;
        }

        var (_, hostProcess) = StartHostProcess(detect);
        await hostProcess.WaitForExitAsync(cancellationToken);

        return Capability.FromInteger(hostProcess.ExitCode);
    }

    private static (HostStartInfo startInfo, Process hostProcess) StartHostProcess(ScriptAction detect)
    {
        HostStartInfo startInfo = detect.CreateHostStartInfo();
        return (startInfo, Process.Start(new ProcessStartInfo()
        {
            FileName = startInfo.Filename,
            Arguments = startInfo.Arguments,
            CreateNoWindow = true,
        }).AssertNotNull());
    }

    public Capability? DetectCapability(TimeSpan timeout)
    {
        if (TryGetValue(Capability.Detect, out var detect))
        {
            var (_, hostProcess) = StartHostProcess(detect);

            if (hostProcess.WaitForExit(Convert.ToInt32(timeout.TotalMilliseconds)))
            {
                return Capability.FromInteger(hostProcess.ExitCode);
            }
            // Detection took too long : kill process.
            hostProcess.Kill(true);
        }

        return null;
    }

}
