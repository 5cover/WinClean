using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Model.Serialization;
using Scover.WinClean.Model.Serialization.Xml;

namespace Scover.WinClean.Services;

public sealed class MetadatasProvider : IMetadatasProvider
{
    private const string MetadataContentFilesNamespace = $"{nameof(Scover)}.{nameof(WinClean)}";

    private readonly Lazy<TypedEnumerableDictionary> _metadatas = new(() =>
    {
        IScriptMetadataDeserializer d = new ScriptMetadataXmlDeserializer();
        return new()
        {
            // Explicitly enumerate the metadata lists.
            d.GetCategories(ReadContentFile("Categories.xml")).ToList(),
            d.GetHosts(ReadContentFile("Hosts.xml")).ToList(),
            d.GetImpacts(ReadContentFile("Impacts.xml")).ToList(),
            d.GetSafetyLevels(ReadContentFile("SafetyLevels.xml")).ToList()
        };
    });

    public TypedEnumerableDictionary Metadatas => _metadatas.Value;

    public T GetMetadata<T>(string invariantName) where T : ScriptLocalizedStringMetadata
        => Metadatas.Get<T>().Single(m => CompareMetadatas(invariantName, m));

    public T? GetMetadataOrDefault<T>(string invariantName) where T : ScriptLocalizedStringMetadata
        => Metadatas.Get<T>().SingleOrDefault(m => CompareMetadatas(invariantName, m));

    private static bool CompareMetadatas(string invariantName, ScriptLocalizedStringMetadata m) => m.InvariantName.Equals(invariantName, StringComparison.OrdinalIgnoreCase);

    private static Stream ReadContentFile(string filename)
        => ServiceProvider.Get<IApplicationInfo>().Assembly.GetManifestResourceStream($"{MetadataContentFilesNamespace}.{filename}").NotNull();
}