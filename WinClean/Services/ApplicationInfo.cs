using System.Reflection;

namespace Scover.WinClean.Services;

public sealed class ApplicationInfo : IApplicationInfo
{
    public Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
    public string Name => Assembly.GetCustomAttribute<AssemblyProductAttribute>().NotNull().Product;
    public string RepositoryUrl => (Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(metadata => metadata.Key == "RepositoryUrl")?.Value).NotNull();
    public string Version => Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().NotNull().InformationalVersion;
}