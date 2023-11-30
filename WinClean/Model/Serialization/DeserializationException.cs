using Scover.WinClean.Resources;

namespace Scover.WinClean.Model.Serialization;

public class DeserializationException : Exception
{
    public DeserializationException(string targetName, Exception? innerException = null)
        : base(ExceptionMessages.DeserializationFailed.FormatWith(targetName), innerException)
    {
    }
}