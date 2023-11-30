namespace Scover.WinClean.Model.Serialization;

public sealed class DeserializationChainException : DeserializationException
{
    public DeserializationChainException(string targetName, IReadOnlyDictionary<string, Exception> deserializerExceptions)
        : base(targetName)
        => DeserializerExceptions = deserializerExceptions;

    public IReadOnlyDictionary<string, Exception> DeserializerExceptions { get; }

    public override string ToString() => string.Concat(DeserializerExceptions.Select(kv => $"{kv.Key}: {kv.Value.Message}{Environment.NewLine}")) + base.ToString();
}