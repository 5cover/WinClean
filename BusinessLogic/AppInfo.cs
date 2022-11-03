using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;

using Scover.WinClean.BusinessLogic.Scripts;
using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Properties;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Holds data related to the Business Logic layer.</summary>
public static class AppInfo
{
    // Setting initialized here because an XML default value doesn't work.
    static AppInfo() => PersistentSettings["ScriptExecutionTimes"] = new SerializableStringDictionary();

    private static readonly IScriptMetadataDeserializer _deserializer = new ScriptMetadataXmlDeserializer();
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    /// <summary>Gets a dictionary that contains all available script hosts, keyed by <see cref="IScriptData.InvariantName"/>.</summary>
    public static IDictionary<string, Host> Hosts { get; } = MakeDictionary(new Host[]
    {
        Host.Cmd,
        Host.PowerShell,
        Host.Regedit
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

    public static Settings Settings => Settings.Default;

    /// <summary>Gets the settings that should not be reset.</summary>
    public static PersistentSettings PersistentSettings => PersistentSettings.Default;

    private static IDictionary<string, T> MakeDictionary<T>(IEnumerable<T> source) where T : IScriptData
                    => new Dictionary<string, T>(source.Select(c => new KeyValuePair<string, T>(c.InvariantName, c)));

    private static Stream OpenContentFile(string filename)
#if PORTABLE
        => Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Join('.', nameof(Scover), nameof(WinClean), filename)).AssertNotNull();
#else
        => File.OpenRead(filename);

#endif

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