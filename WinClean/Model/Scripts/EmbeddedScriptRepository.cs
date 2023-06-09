using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Services;

namespace Scover.WinClean.Model.Scripts;

public class EmbeddedScriptRepository : ScriptRepository
{
    private readonly string _namespace;
    private readonly List<Script> _scripts = new();

    /// <param name="namespace">The namespace of each manifest resource.</param>
    public EmbeddedScriptRepository(string @namespace, IScriptSerializer serializer, ScriptType type) : base(serializer, type) =>
        // Add a dot to only load resources inside the namespace.
        _namespace = @namespace + '.';

    public override int Count => _scripts.Count;

    public override IEnumerator<Script> GetEnumerator() => _scripts.GetEnumerator();

    protected override void LoadScripts()
    {
        var assembly = ServiceProvider.Get<IApplicationInfo>().Assembly;
        foreach (var resName in assembly.GetManifestResourceNames().Where(name => name.StartsWith(_namespace, StringComparison.Ordinal)))
        {
            using Stream stream = assembly.GetManifestResourceStream(resName).NotNull();

            _scripts.Add(Serializer.Deserialize(Type, stream));
        }
    }
}