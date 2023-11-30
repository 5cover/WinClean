using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Services;

public interface IMetadatasProvider
{
    StringComparison Comparison { get; }
    TypedEnumerableDictionary Metadatas { get; }

    T GetMetadata<T>(string invariantName) where T : Metadata;
}