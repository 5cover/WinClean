namespace Scover.WinClean.BusinessLogic;

/// <summary>
/// An exception thrown by the filesystem.
/// </summary>
public class FileSystemException : Exception
{
    public FileSystemException(Exception innerException, FSVerb verb, string element, string? message = null) : base(message ?? "A filesystem exception occured.", innerException)
        => (Element, Verb) = (element, verb);
    public string Element { get; }
    public FSVerb Verb { get; }
}
