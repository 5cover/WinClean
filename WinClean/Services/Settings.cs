using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using Scover.WinClean.Properties;

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
        SetComputedAppSettings();
    }

    public SemVersionRange DefaultScriptSupportedVersionRange { get; private set; }
    public double Height { get => AppSettings.Height; set => AppSettings.Height = value; }
    public bool IsLoggingEnabled { get => AppSettings.IsLoggingEnabled; set => AppSettings.IsLoggingEnabled = value; }
    public bool IsMaximized { get => AppSettings.IsMaximized; set => AppSettings.IsMaximized = value; }
    public double Left { get => AppSettings.Left; set => AppSettings.Left = value; }
    public IDictionary<string, TimeSpan> ScriptExecutionTimes { get; private set; }
    public string ScriptFileExtension => AppSettings.ScriptFileExtension;
    public TimeSpan ScriptTimeout { get => AppSettings.ScriptTimeout; set => AppSettings.ScriptTimeout = value; }
    public bool ShowUpdateDialog { get => AppSettings.ShowUpdateDialog; set => AppSettings.ShowUpdateDialog = value; }
    public double Top { get => AppSettings.Top; set => AppSettings.Top = value; }
    public double Width { get => AppSettings.Width; set => AppSettings.Width = value; }

    private static string ScriptExecutionTimesSeparator => Environment.NewLine;

    private Properties.Settings AppSettings { get; } = new();

    private PersistentSettings PersistentSettings { get; } = new();

    public void Reset()
    {
        AppSettings.Reset();
        SetComputedAppSettings();
    }

    public void Save()
    {
        AppSettings.Save();
        PersistentSettings.Save();
    }

    private static IEnumerable<KeyValuePair<string, string>> ParseMockDictionary(string str, string separator)
    {
        if (str == "")
        {
            yield break;
        }
        var keysAndValues = str.Split(separator);
        if (keysAndValues.Length % 2 == 1)
        {
            throw new ArgumentException("Not every key matches with a value", nameof(str));
        }
        for (int i = 0; i < keysAndValues.Length; i += 2)
        {
            yield return new(keysAndValues[i], keysAndValues[i + 1]);
        }
    }

    private static string ToMockStringDic<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, string separator, Func<TKey, string>? keyFormatter = null, Func<TValue, string>? valueFormatter = null)
    {
        return separator == ""
            ? throw new ArgumentException("Separator is empty", nameof(separator))
            : new StringBuilder().AppendJoin(separator, keyValuePairs.SelectMany(kv => Zip(Format(kv.Key, keyFormatter), Format(kv.Value, valueFormatter)))).ToString();

        string? Format<T>(T t, Func<T, string>? formatter)
        {
            var tstr = formatter?.Invoke(t) ?? t?.ToString();
            return tstr?.Contains(separator) ?? false
                ? throw new ArgumentException("One of formatted keys or values contain the separator", nameof(keyValuePairs))
                : tstr;
        }
        static IEnumerable<string?> Zip(string? s1, string? s2)
        {
            yield return s1;
            yield return s2;
        }
    }

    [MemberNotNull(nameof(DefaultScriptSupportedVersionRange))]
    private void SetComputedAppSettings() => DefaultScriptSupportedVersionRange = SemVersionRange.Parse(AppSettings.DefaultScriptSupportedVersionRange);

    public TimeSpan ScriptDetectionTimeout { get => AppSettings.ScriptDetectionTimeout; set => AppSettings.ScriptDetectionTimeout = value; }
}