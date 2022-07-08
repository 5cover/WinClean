namespace Scover.WinClean.DataAccess;

/// <summary>Represents an XML file containing data related to the application.</summary>
/// <typeparam name="T">The type of the data contained in the XML file.</typeparam>
public interface IAppXmlFile<T>
{
    public AppDirectory Directory { get; }
    public string Name { get; }

    public T Deserialize();

    public void Serialize(T data);
}