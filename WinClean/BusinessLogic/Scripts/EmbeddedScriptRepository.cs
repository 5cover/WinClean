using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts;

public class EmbeddedScriptRepository : ScriptRepository
{
    private readonly string _namespace;
    private readonly List<Script> _scripts = new();
    private readonly IScriptSerializer _serializer;

    /// <param name="namespace">The namespace of each manifest resource.</param>
    public EmbeddedScriptRepository(ScriptType type, string @namespace, IScriptSerializer serializer) : base(type)
    {
        _serializer = serializer;
        // Add a dot to only load resources inside the namespace.
        _namespace = @namespace + '.';
    }

    public override int Count => _scripts.Count;

    public override IEnumerator<Script> GetEnumerator() => _scripts.GetEnumerator();

    protected override void LoadScripts()
    {
        foreach (var resName in AppInfo.Assembly.GetManifestResourceNames().Where(name => name.StartsWith(_namespace, StringComparison.Ordinal)))
        {
            Stream stream;
            stream = AppInfo.Assembly.GetManifestResourceStream(resName).AssertNotNull();
            _scripts.Add(_serializer.Deserialize(Type, stream));
        }
    }
}