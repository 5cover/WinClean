using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Scripts.Hosts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Properties;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Holds data related to the Business Logic layer.</summary>
public static class AppInfo
{
    private static readonly IScriptMetadataDeserializer _deserializer = new ScriptMetadataXmlDeserializer();
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    /// <summary>Gets a dictionary that contains all available script hosts, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static IDictionary<string, IHost> Hosts { get; } = MakeDictionary(new IHost[]
    {
        ProcessHost.Cmd,
        PowerShell.Instance,
        ProcessHost.Regedit
    });

    #region Lazy script metadata

    /// <summary>Gets a dictionary that contains all available script categories, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, Category>> Categories { get; }
        = new(() => MakeDictionary(_deserializer.MakeCategories(OpenContentFile("Categories.xml"))));

    /// <summary>Gets a dictionary that contains all available script impacts, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, Impact>> Impacts { get; }
        = new(() => MakeDictionary(_deserializer.MakeImpacts(OpenContentFile("Impacts.xml"))));

    /// <summary>Gets a dictionary that contains all available script recommendation levels, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, RecommendationLevel>> RecommendationLevels { get; }
        = new(() => MakeDictionary(_deserializer.MakeRecommendationLevels(OpenContentFile("RecommendationLevels.xml"))));

    #endregion Lazy script metadata

    /// <summary>Gets or sets the error callback for opening application files.</summary>
    /// <remarks>This property must be set externally in the Presentation layer.</remarks>
    /// <returns>
    /// <see langword="true"/> if the filesystem operation should be retried; <see langword="false"/> if it should fail and
    /// throw <paramref name="exception"/>.
    /// </returns>
    public static FSOperationCallback OpenAppFileRetryElseFail { get; set; } = (_, _, _a)
        => throw new NotSupportedException(Resources.DevException.CallbackNotSet.FormatWith(nameof(OpenAppFileRetryElseFail)));

    public static Settings Settings => Settings.Default;

    private static IDictionary<string, T> MakeDictionary<T>(IEnumerable<T> source) where T : IScriptData
                    => new Dictionary<string, T>(source.Select(c => new KeyValuePair<string, T>(c.InvariantName, c)));

    private static Stream OpenContentFile(string filename)
    {
        while (true)
        {
            try
            {
#if PORTABLE
                return Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Join('.', nameof(Scover), nameof(WinClean), filename)).AssertNotNull();
#else
                return File.OpenRead(filename);
#endif
            }
            catch (Exception e) when (e.IsFileSystem())
            {
                if (!OpenAppFileRetryElseFail(e, FSVerb.Access, new FileInfo(filename)))
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