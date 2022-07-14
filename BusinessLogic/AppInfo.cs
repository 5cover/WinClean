using System.Globalization;
using System.Reflection;
using System.Resources;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Properties;

namespace Scover.WinClean.BusinessLogic;

public static class AppInfo
{
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    #region Assembly attributes

    public static IReadOnlyCollection<Category> Categories { get; } = MakeFactory("Categories.xml").MakeCategories().ToList();

    public static IReadOnlyCollection<Host> Hosts { get; } = new[]
    {
        Host.Cmd,
        Host.PowerShell,
        Host.Regedit
    };

    public static IReadOnlyCollection<Impact> Impacts { get; } = MakeFactory("Impacts.xml").MakeImpacts().ToList();
    public static string Name => assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? string.Empty;
    public static CultureInfo NeutralResourcesCulture => new(assembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>()?.CultureName ?? string.Empty);
    public static FSOperationCallback ReadAppFileRetryOrFail { get; set; } = (_, _, _) => throw new NotSupportedException($"{nameof(ReadAppFileRetryOrFail)} callback not set");
    public static IReadOnlyCollection<RecommendationLevel> RecommendationLevels { get; } = MakeFactory("RecommendationLevels.xml").MakeRecommendationLevels().ToList();
    public static string? RepositoryUrl => assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(metadata => metadata.Key == "RepoUrl")?.Value;
    public static Settings Settings => Settings.Default;
    public static string Version => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? string.Empty;

    #endregion Assembly attributes

    private static IScriptMetadataDeserializer MakeFactory(string filename) => new ScriptMetadataXmlDeserializer(ReadAppFile(filename));

    private static string ReadAppFile(string filename)
    {
        string path = AppDirectory.InstallDir.Join(filename);
        while (true)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e) when (e.IsFileSystem())
            {
                if (!ReadAppFileRetryOrFail(e, FSVerb.Access, new FileInfo(path)))
                {
                    throw;
                }
            }
        }
    }
}