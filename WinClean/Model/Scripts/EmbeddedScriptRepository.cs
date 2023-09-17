using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Services;

namespace Scover.WinClean.Model.Scripts;

public class EmbeddedScriptRepository : ScriptRepository
{
    private readonly string _namespace;

    /// <param name="namespace">The namespace of each manifest resource.</param>
    public EmbeddedScriptRepository(string @namespace, IScriptSerializer serializer, ScriptType type) : base(serializer, type) =>
        // Add a dot to only load resources inside the namespace.
        _namespace = @namespace + '.';

    public override Script GetScript(string source)
    {
        using Stream? stream  = ServiceProvider.Get<IApplicationInfo>().Assembly.GetManifestResourceStream(source);
        return Serializer.Deserialize(Type, stream ?? throw new ArgumentException($"The script at {source} could not be found.", nameof(source)));
    }

    public override Task LoadAsync()
    {
        foreach (var resName in ServiceProvider.Get<IApplicationInfo>().Assembly.GetManifestResourceNames().Where(name => name.StartsWith(_namespace, StringComparison.Ordinal)))
        {
            AddItem(GetScript(resName));
        }
        return Task.CompletedTask;
    }
}