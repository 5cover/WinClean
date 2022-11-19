using System.Reflection;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts created from embedded resources.</summary>
public sealed class ManifestResourceScriptCollection : ScriptCollection
{
    public ManifestResourceScriptCollection(IScriptSerializer serializer, ScriptType scriptType) : base(serializer, scriptType)
    {
    }

    /// <param name="source">The name of the embedded resource.</param>
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="source"/> is an unknown or invalid embedded resource name.</exception>
    public override void Load(string source)
    {
        Stream stream;
        try
        {
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(source)
                ?? throw new ArgumentException($"Unknown embedded resource name: '{source}'", nameof(source));
        }
        catch (Exception e) when (e is ArgumentException or FileNotFoundException or BadImageFormatException or NotImplementedException)
        {
            throw new ArgumentException($"Invalid embedded resource name: '{source}'", nameof(source), e);
        }
        Sources.Add(Deserialize(stream), source);
    }
}