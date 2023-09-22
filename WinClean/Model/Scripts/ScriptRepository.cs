using System.Collections.ObjectModel;
using System.Diagnostics;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;

namespace Scover.WinClean.Model.Scripts;

[DebuggerDisplay($"{{{nameof(Type)}, {nameof(Scripts)}}}")]
public abstract class ScriptRepository
{
    protected ScriptRepository(IScriptSerializer serializer, ScriptType type) => (Serializer, Type) = (serializer, type);

    public ObservableCollection<Script> Scripts { get; } = new();
    public ScriptType Type { get; }
    protected IScriptSerializer Serializer { get; }

    public abstract Task LoadAsync();

    /// <summary>Retrieves the script at the specified source.</summary>
    /// <param name="source">The source of the script to retrieve.</param>
    /// <exception cref="DeserializationException">Script deserialization failed.</exception>
    /// <exception cref="ArgumentException">
    /// The script at <paramref name="source"/> could not be found.
    /// </exception>
    /// <exception cref="FileSystemException">A filesystem error occured.</exception>
    /// <returns>A new <see cref="Script"/> object.</returns>
    public abstract Script RetrieveScript(string source);
}