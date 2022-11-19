using System.Reflection;

using Scover.WinClean.BusinessLogic.Xml;
using Scover.WinClean.DataAccess;
using Scover.WinClean.Properties;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Holds data related to the Business Logic layer.</summary>
public static class AppInfo
{
    // Setting initialized here because an XML default value doesn't work.
    static AppInfo() => PersistentSettings["ScriptExecutionTimes"] = new SerializableStringDictionary();

    private static readonly IScriptMetadataDeserializer deserializer = new ScriptMetadataXmlDeserializer();
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    #region Script metadata

    /// <summary>Gets a lazy dictionary that contains all available script categories, keyed by <see cref="ScriptMetadata.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, Category>> Categories { get; } = Deserialize(deserializer.MakeCategories, "Categories.xml");

    /// <summary>Gets a lazy dictionary that contains all available script hosts, keyed by <see cref="ScriptMetadata.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, Host>> Hosts { get; } = Deserialize(deserializer.MakeHosts, "Hosts.xml");

    /// <summary>Gets a lazy dictionary that contains all available script impacts, keyed by <see cref="ScriptMetadata.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, Impact>> Impacts { get; } = Deserialize(deserializer.MakeImpacts, "Impacts.xml");

    /// <summary>Gets a lazy dictionary that contains all available script recommendation levels, keyed by <see cref="ScriptMetadata.InvariantName"/>.</summary>
    public static Lazy<IDictionary<string, RecommendationLevel>> RecommendationLevels { get; } = Deserialize(deserializer.MakeRecommendationLevels, "RecommendationLevels.xml");

    #endregion Script metadata

    private static Lazy<IDictionary<string, T>> Deserialize<T>(Func<Stream, IEnumerable<T>> deserialize, string filename) where T : ScriptMetadata
        => new(() => new Dictionary<string, T>(deserialize(OpenContentFile(filename)).Select(c => new KeyValuePair<string, T>(c.InvariantName, c))));

    public static Settings Settings => Settings.Default;

    /// <summary>Gets the settings that should not be reset.</summary>
    public static PersistentSettings PersistentSettings => PersistentSettings.Default;

    private static Stream OpenContentFile(string filename)
#if PORTABLE
        => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{nameof(Scover)}.{nameof(WinClean)}.{filename}").AssertNotNull();
#else
        => File.OpenRead(filename);

#endif

    #region Assembly attributes

    public static string Name
        => assembly.GetCustomAttribute<AssemblyProductAttribute>().AssertNotNull().Product;

    public static string RepositoryUrl
        => (assembly.GetCustomAttributes<AssemblyMetadataAttribute>().SingleOrDefault(metadata => metadata.Key == "RepositoryUrl")?.Value).AssertNotNull();

    public static string Version
        => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().AssertNotNull().InformationalVersion;

    #endregion Assembly attributes
}