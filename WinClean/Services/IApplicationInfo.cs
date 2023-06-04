using System.Reflection;

namespace Scover.WinClean.Services;

/// <summary>Retrieves application information and metadata (such as assembly attributes).</summary>
public interface IApplicationInfo
{
    public Assembly Assembly { get; }

    /// <summary>Gets product name information.</summary>
    /// <value><see cref="AssemblyProductAttribute.Product"/>.</value>
    public string Name { get; }

    /// <summary>Gets the <c>RepositoryUrl</c> assembly metadata.</summary>
    /// <value>
    /// <see cref="AssemblyMetadataAttribute.Value"/> for <see cref="AssemblyMetadataAttribute.Key"/> =
    /// <c>RepositoryUrl</c>.
    /// </value>
    public string RepositoryUrl { get; }

    /// <summary>Gets version information.</summary>
    /// <value><see cref="AssemblyInformationalVersionAttribute.InformationalVersion"/>.</value>
    public string Version { get; }
}