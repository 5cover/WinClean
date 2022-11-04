using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Scover.WinClean.DataAccess;

// https://bytes.com/topic/c-sharp/answers/492245-complex-data-types-user-scoped-applicationsettings#post1906133
[SuppressMessage("Design", "CA1010")]
[SuppressMessage("Naming", "CA1711")]
public class SerializableStringDictionary : StringDictionary, IXmlSerializable
{
    private static readonly XmlSerializer _x = new(typeof(List<Node>), new[] { typeof(Node) });

    [Serializable]
    public record Node
    {
        public Node() => Key = string.Empty;

        public Node(string key, string? val) => (Key, Val) = (key, val);
        public string Key { get; }
        public string? Val { get; }
    }

    public System.Xml.Schema.XmlSchema? GetSchema() => null;

    public void ReadXml(System.Xml.XmlReader reader)
    {
        _ = reader.Read();
        if (_x.Deserialize(reader) is not List<Node> list)
        {
            return;
        }
        foreach (Node node in list)
        {
            Add(node.Key, node.Val);
        }
    }

    public void WriteXml(System.Xml.XmlWriter writer)
    {
        List<Node> list = new();
        foreach (string key in Keys)
        {
            list.Add(new Node(key, this[key]));
        }
        _x.Serialize(writer, list);
    }
}