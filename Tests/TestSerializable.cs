using System.Text;

namespace Tests;

public abstract class TestSerializable<T>
{
    protected TestSerializable(StringBuilder xml, T value) => (Xml, Value) = (xml, value);

    public T Value { get; }
    public StringBuilder Xml { get; }

    protected static StringBuilder Element(string name, StringBuilder contents, params (string name, string value)[] attributes)
        => Element(name, contents.ToString(), attributes);

    protected static StringBuilder Element(string name, string contents, params (string name, string value)[] attributes)
    => new($"<{name}{FormatAttributes(attributes)}>{contents}</{name}>");

    protected static StringBuilder Element(string name, params (string name, string value)[] attributes)
        => new($"<{name}{FormatAttributes(attributes)}/>");

    private static StringBuilder FormatAttributes((string name, string value)[] attributes)
        => attributes.Aggregate(new StringBuilder(), (sum, a) => sum.Append(@$" {a.name}=""{a.value}"""));
}