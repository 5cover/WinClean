namespace Scover.WinClean.Model.Serialization;

public class DeserializationException : Exception
{
    public DeserializationException(string targetName, string? erroneousData = null, Exception? innerException = null)
        : base($"The data could not be deserialized to a {targetName} because it is invalid or incomplete.", innerException)
        => ErroneousData = erroneousData;

    public string? ErroneousData { get; }
}