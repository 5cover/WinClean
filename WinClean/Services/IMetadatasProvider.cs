using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Services;

public interface IMetadatasProvider
{
    TypedEnumerableDictionary Metadatas { get; }

    T GetMetadata<T>(string invariantName) where T : ScriptLocalizedStringMetadata;

    T? GetMetadataOrDefault<T>(string invariantName) where T : ScriptLocalizedStringMetadata;
}