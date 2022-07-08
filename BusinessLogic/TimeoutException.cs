using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

public class TimeoutException : Exception
{
    public TimeoutException() : base(Resources.DevException.Timeout)
    {
    }

    public TimeoutException(string message) : base(message)
    {
    }

    public TimeoutException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public TimeoutException(TimeSpan timeout, string message) : base(message) => Timeout = timeout;

    public TimeoutException(TimeSpan timeout) : base(Resources.DevException.TimeoutSpecified.FormatWithInvariant(timeout)) => Timeout = timeout;

    public TimeSpan Timeout { get; }
}