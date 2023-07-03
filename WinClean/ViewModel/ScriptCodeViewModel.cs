using System.Collections;
using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Services;

namespace Scover.WinClean.ViewModel;

public sealed class ScriptCodeViewModel : ObservableObject, IDictionary<Capability, ScriptAction>
{
    private readonly ScriptCode _model;

    public ScriptCodeViewModel(ScriptCode model)
    {
        _model = model;
        EffectiveCapability = new(() => _model.DetectCapability(ServiceProvider.Get<ISettings>().ScriptDetectionTimeout), ct => _model.DetectCapabilityAsync(ct));
    }

    public int Count => _model.Count;

    /// <remarks>Use IsAsync=true when binding to this property.</remarks>
    /// <summary>Executes detection synchronously once.</summary>
    public Cached<Capability?> EffectiveCapability { get; }

    public bool IsReadOnly => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_model).IsReadOnly;

    public ICollection<Capability> Keys => _model.Keys;

    public ICollection<ScriptAction> Values => _model.Values;

    public ScriptAction this[Capability key] { get => _model[key]; set => _model[key] = value; }

    public void Add(Capability key, ScriptAction value) => _model.Add(key, value);

    public bool ContainsKey(Capability key) => _model.ContainsKey(key);

    public async Task<Capability?> DetectCapabilityAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource cts = new();
        using var reg = cancellationToken.Register(cts.Cancel);
        cts.CancelAfter(ServiceProvider.Get<ISettings>().ScriptDetectionTimeout);
        return await _model.DetectCapabilityAsync(cts.Token);
    }

    public IEnumerator<KeyValuePair<Capability, ScriptAction>> GetEnumerator() => _model.GetEnumerator();

    public bool Remove(Capability key) => _model.Remove(key);

    public bool TryGetValue(Capability key, [MaybeNullWhen(false)] out ScriptAction value) => _model.TryGetValue(key, out value);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.Add(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_model).Add(item);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.Clear() => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_model).Clear();

    bool ICollection<KeyValuePair<Capability, ScriptAction>>.Contains(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_model).Contains(item);

    void ICollection<KeyValuePair<Capability, ScriptAction>>.CopyTo(KeyValuePair<Capability, ScriptAction>[] array, int arrayIndex) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_model).CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_model).GetEnumerator();

    bool ICollection<KeyValuePair<Capability, ScriptAction>>.Remove(KeyValuePair<Capability, ScriptAction> item) => ((ICollection<KeyValuePair<Capability, ScriptAction>>)_model).Remove(item);
}