using System.Globalization;
using System.Reflection;
using System.Resources;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Scripts.Hosts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Properties;

namespace Scover.WinClean.BusinessLogic;

public static class AppInfo
{
    private static readonly IScriptMetadataDeserializer _deserializer = new ScriptMetadataXmlDeserializer();
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    #region Script metadata

    /// <summary>Gets a dictionary that contains all available script categories, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static IDictionary<string, Category> Categories { get; }
        = MakeDictionary(_deserializer.MakeCategories(OpenAppFile("Categories.xml")));

    /// <summary>Gets a dictionary that contains all available script hosts, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static IDictionary<string, IHost> Hosts { get; } = MakeDictionary(new IHost[]
    {
        ProcessHost.Cmd,
        PowerShell.Instance,
        ProcessHost.Regedit
    });

    /// <summary>Gets a dictionary that contains all available script impacts, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static IDictionary<string, Impact> Impacts { get; }
        = MakeDictionary(_deserializer.MakeImpacts(OpenAppFile("Impacts.xml")));

    /// <summary>Gets a dictionary that contains all available script recommendation levels, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static IDictionary<string, RecommendationLevel> RecommendationLevels { get; }
        = MakeDictionary(_deserializer.MakeRecommendationLevels(OpenAppFile("RecommendationLevels.xml")));

    #endregion Script metadata

    /// <summary>Gets or sets the error callback for reading application files.</summary>
    /// <remarks>This property must be set externally in the Presentation layer.</remarks>
    public static FSOperationCallback ReadAppFileRetryOrFail { get; set; } = (_, _, _)
        => throw new NotSupportedException(Resources.DevException.CallbackNotSet.FormatWith(nameof(ReadAppFileRetryOrFail)));

    public static Settings Settings => Settings.Default;

    private static Dictionary<string, T> MakeDictionary<T>(IEnumerable<T> source) where T : IScriptData
                    => new(source.Select(c => new KeyValuePair<string, T>(c.InvariantName, c)));

    private static Stream OpenAppFile(string filename)
    {
        string path = AppDirectory.InstallDir.Join(filename);
        while (true)
        {
            try
            {
                return File.OpenRead(path);
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

    #region Assembly attributes

    public static string Name
        => assembly.GetCustomAttribute<AssemblyProductAttribute>().AssertNotNull().Product;

    public static CultureInfo NeutralResourcesCulture
        => new(assembly.GetCustomAttribute<NeutralResourcesLanguageAttribute>().AssertNotNull().CultureName);

    public static string RepositoryUrl
        => (assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(metadata => metadata.Key == "RepositoryUrl")?.Value).AssertNotNull();

    public static string Version
        => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().AssertNotNull().InformationalVersion;

    #endregion Assembly attributes
}