using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;

namespace Scover.WinClean.Model;

/// <summary>An exception thrown by the filesystem.</summary>
public class FileSystemException : Exception
{
    public FileSystemException(Exception innerException, FSVerb verb, string element, string? message = null) : base(message ?? ExceptionMessages.FileSystemException, innerException)
        => (Element, Verb) = (element, verb);

    public string Element { get; }
    public FSVerb Verb { get; }
}