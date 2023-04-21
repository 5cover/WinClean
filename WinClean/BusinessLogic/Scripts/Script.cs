using System.ComponentModel;
using System.Runtime.CompilerServices;

using Scover.WinClean.DataAccess;

using Semver;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic.Scripts;

public class Script : INotifyPropertyChanged, IEquatable<Script?>
{
    private Category _category;
    private string _code;
    private Host _host;
    private Impact _impact;
    private RecommendationLevel _recommendationLevel;

    private SemVersionRange _supportedVersionsRange;

    public Script(Category category, string code, Host host, Impact impact, SemVersionRange supportedVersionsRange, RecommendationLevel recommendationLevel, LocalizedString localizedDescription, LocalizedString localizedName, ScriptType type)
            => (_category, _code, _host, _impact, _supportedVersionsRange, _recommendationLevel, LocalizedDescription, LocalizedName, Type)
         = (category, code, host, impact, supportedVersionsRange, recommendationLevel, localizedDescription, localizedName, type);

    public event PropertyChangedEventHandler? PropertyChanged;

    public Category Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
        }
    }

    public bool IsIncompatibleWithCurrentVersion => !SemVersion.FromVersion(Environment.OSVersion.Version).Satisfies(Versions);

    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => LocalizedDescription.Get(CurrentUICulture);
        set
        {
            LocalizedDescription.Set(CurrentUICulture, value);
            OnPropertyChanged();
        }
    }

    public Host Host
    {
        get => _host;
        set
        {
            _host = value;
            OnPropertyChanged();
        }
    }

    public Impact Impact
    {
        get => _impact;
        set
        {
            _impact = value;
            OnPropertyChanged();
        }
    }

    public string InvariantName
    {
        get => LocalizedName.Get(InvariantCulture);
        set
        {
            LocalizedName.Set(InvariantCulture, value);
            OnPropertyChanged();
        }
    }

    public LocalizedString LocalizedDescription { get; init; }
    public LocalizedString LocalizedName { get; init; }

    public string Name
    {
        get => LocalizedName.Get(CurrentUICulture);
        set
        {
            LocalizedName.Set(CurrentUICulture, value);
            OnPropertyChanged();
        }
    }

    public RecommendationLevel RecommendationLevel
    {
        get => _recommendationLevel;
        set
        {
            _recommendationLevel = value;
            OnPropertyChanged();
        }
    }

    public SemVersionRange Versions
    {
        get => _supportedVersionsRange;
        set
        {
            _supportedVersionsRange = value;
            OnPropertyChanged();
        }
    }

    public ScriptType Type { get; }

    public override bool Equals(object? obj) => Equals(obj as Script);

    public bool Equals(Script? other) => other is not null && InvariantName == other.InvariantName;

    /// <summary>Executes the script.</summary>
    /// <inheritdoc cref="Host.Execute(string)"/>
    public ScriptExecution Execute()
        => Host.Execute(Code);

    public override int GetHashCode() => InvariantName.GetHashCode();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
}