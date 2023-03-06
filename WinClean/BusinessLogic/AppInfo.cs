using System.Reflection;

using Scover.WinClean.DataAccess;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Shortucts to assembly attributes and metadata.</summary>
public static class AppInfo
{
    public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    /// <summary>Gets product name information.</summary>
    /// <value><see cref="AssemblyProductAttribute.Product"/>.</value>
    public static string Name { get; } = Assembly.GetCustomAttribute<AssemblyProductAttribute>().AssertNotNull().Product;

    /// <summary>Gets the <c>RepositoryUrl</c> assembly metadata.</summary>
    /// <value>
    /// <see cref="AssemblyMetadataAttribute.Value"/> for <see cref="AssemblyMetadataAttribute.Key"/> as
    /// <c>RepositoryUrl</c>.
    /// </value>
    public static string RepositoryUrl { get; } = (Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(metadata => metadata.Key == "RepositoryUrl")?.Value).AssertNotNull();

    /// <summary>Gets version information.</summary>
    /// <value><see cref="AssemblyInformationalVersionAttribute.InformationalVersion"/>.</value>
    public static string Version { get; } = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().AssertNotNull().InformationalVersion;
}