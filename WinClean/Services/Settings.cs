using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using Scover.WinClean.Properties;
using Scover.WinClean.Resources;

using Semver;

namespace Scover.WinClean.Services;

public sealed class Settings : ISettings
{
    private const string ScriptExecutionTimesFormatString = "c";

    public Settings()
    {
        ScriptExecutionTimes = ParseMockDictionary(PersistentSettings.ScriptExecutionTimes, ScriptExecutionTimesSeparator)
            .ToDictionary(kv => kv.Key, kv => TimeSpan.ParseExact(kv.Value, ScriptExecutionTimesFormatString, CultureInfo.InvariantCulture));

        PersistentSettings.SettingsSaving += (s, e) => PersistentSettings.ScriptExecutionTimes = ToMockStringDic(ScriptExecutionTimes, ScriptExecutionTimesSeparator);
        SetComputedSettings();
    }

    public SemVersionRange DefaultHostVersions { get; private set; }
    public SemVersionRange DefaultScriptVersions { get; private set; }
    public double Height { get => AppSettings.Height; set => AppSettings.Height = value; }
    public bool IsLoggingEnabled { get => AppSettings.IsLoggingEnabled; set => AppSettings.IsLoggingEnabled = value; }
    public bool IsMaximized { get => AppSettings.IsMaximized; set => AppSettings.IsMaximized = value; }
    public string LatestVersionUrl => AppSettings.LatestVersionUrl;
    public double Left { get => AppSettings.Left; set => AppSettings.Left = value; }
    public string NewIssueUrl => AppSettings.NewIssueUrl;
    public long RepositoryId => AppSettings.RepositoryId;
    public TimeSpan ScriptDetectionTimeout => AppSettings.ScriptDetectionTimeout;
    public IDictionary<string, TimeSpan> ScriptExecutionTimes { get; private set; }
    public string ScriptFileExtension => AppSettings.ScriptFileExtension;
    public bool ShowUpdateDialog { get => AppSettings.ShowUpdateDialog; set => AppSettings.ShowUpdateDialog = value; }
    public double Top { get => AppSettings.Top; set => AppSettings.Top = value; }
    public double Width { get => AppSettings.Width; set => AppSettings.Width = value; }

    public string WikiUrl => AppSettings.WikiUrl;
    private static string ScriptExecutionTimesSeparator => Environment.NewLine;

    private Properties.Settings AppSettings { get; } = new();

    private PersistentSettings PersistentSettings { get; } = new();

    public void Reset()
    {
        AppSettings.Reset();
        SetComputedSettings();
    }

    public void Save()
    {
        AppSettings.Save();
        PersistentSettings.Save();
    }

    /// <exception cref="ArgumentException">Not every key matches with a value.</exception>
    private static IEnumerable<KeyValuePair<string, string>> ParseMockDictionary(string str, string separator)
    {
        if (str == "")
        {
            yield break;
        }
        foreach (var kvp in str.Split(separator).Chunk(2))
        {
            yield return new(kvp[0], kvp[1]);
        }
    }

    /// <exception cref="ArgumentException"><paramref name="separator"/> is empty.</exception>
    private static string ToMockStringDic<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> kvps, string separator, Func<TKey, string>? keyFormatter = null, Func<TValue, string>? valueFormatter = null)
    {
        return separator == ""
            ? throw new ArgumentException(ExceptionMessages.EmptyString, nameof(separator))
            : new StringBuilder().AppendJoin(separator, kvps.SelectMany(kv => Zip(Format(kv.Key, keyFormatter), Format(kv.Value, valueFormatter)))).ToString();

        string? Format<T>(T t, Func<T, string>? formatter)
        {
            var formatted = formatter?.Invoke(t) ?? t?.ToString();
            return formatted?.Contains(separator) ?? false
                ? throw new ArgumentException(ExceptionMessages.DataContainsSeparator.FormatWith(formatted, separator), nameof(kvps))
                : formatted;
        }
        static IEnumerable<string?> Zip(string? s1, string? s2)
        {
            yield return s1;
            yield return s2;
        }
    }

    [MemberNotNull(nameof(DefaultScriptVersions), nameof(DefaultHostVersions))]
    private void SetComputedSettings()
    {
        DefaultScriptVersions = SemVersionRange.Parse(AppSettings.DefaultScriptVersions);
        DefaultHostVersions = SemVersionRange.Parse(AppSettings.DefaultHostVersions);
    }
}