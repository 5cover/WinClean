using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Serialization;

public class DeserializationException : Exception
{
    public DeserializationException(string targetName, string? erroneousData = null, Exception? innerException = null)
        : base(ExceptionMessages.DeserializationFailed.FormatWith(targetName), innerException)
        => ErroneousData = erroneousData;

    public string? ErroneousData { get; }
}