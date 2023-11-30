namespace Scover.WinClean.Model;

/// <summary>Information for executing a script host program.</summary>
public class HostStartInfo : IDisposable
{
    public HostStartInfo(string filename, string arguments)
        => (Filename, Arguments) = (filename, arguments);

    public virtual string Arguments { get; }
    public string Filename { get; }

    public virtual void Dispose() => GC.SuppressFinalize(this);
}