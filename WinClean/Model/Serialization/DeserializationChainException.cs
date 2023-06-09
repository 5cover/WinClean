namespace Scover.WinClean.Model.Serialization;

public sealed class DeserializationChainException : DeserializationException
{
    public DeserializationChainException(string targetName, string erroneousData, IDictionary<string, Exception> deserializerExceptions)
        : base(targetName, erroneousData)
        => DeserializerExceptions = deserializerExceptions;

    public IDictionary<string, Exception> DeserializerExceptions { get; }

    public override string ToString() => string.Concat(DeserializerExceptions.Select(kv => $"{kv.Key}: {kv.Value.Message}{Environment.NewLine}")) + base.ToString();
}