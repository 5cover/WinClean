using System.Reflection;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A collection of scripts created from embedded resources.</summary>
public sealed class ManifestResourceScriptCollection : ScriptCollection
{
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    /// <param name="namespace">The namespace of each manifest resource.</param>
    /// <inheritdoc cref="ScriptCollection(IScriptSerializer, ScriptType)"/>
    public ManifestResourceScriptCollection(string @namespace, IScriptSerializer serializer, ScriptType scriptType) : base(serializer, scriptType)
    {
        // Add a dot to only load resources inside the namespace.
        @namespace += '.';
        foreach (var resName in assembly.GetManifestResourceNames().Where(name => name.StartsWith(@namespace, StringComparison.Ordinal)))
        {
            Stream stream;
            try
            {
                stream = assembly.GetManifestResourceStream(resName).AssertNotNull();
            }
            catch (Exception e) when (e is ArgumentException or FileNotFoundException or BadImageFormatException or NotImplementedException)
            {
                throw new ArgumentException($"Invalid embedded resource name: '{resName}'", nameof(@namespace), e);
            }
            Sources.Add(Deserialize(stream), resName);
        }
    }
}