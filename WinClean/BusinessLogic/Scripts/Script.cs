using System.ComponentModel;
using System.Runtime.CompilerServices;
using Scover.WinClean.DataAccess;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>An immutable script deployed with the application.</summary>
public sealed class Script : INotifyPropertyChanged
{
    private Category _category;
    private string _code;
    private Host _host;
    private Impact _impact;
    private RecommendationLevel _recommendationLevel;
    private bool _selected;

    public Script(Category category,
                  string code,
                  Host host,
                  Impact impact,
                  RecommendationLevel recommendationLevel,
                  LocalizedString localizedDescription,
                  LocalizedString localizedName,
                  ScriptType type)
        => (_category, _code, _host, _impact, _recommendationLevel, LocalizedDescription, LocalizedName, Type)
         = (category, code, host, impact, recommendationLevel, localizedDescription, localizedName, type);

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

    /// <summary>Gets or sets whether this script has been selected for execution by the user.</summary>
    public bool IsSelected
    {
        get => _selected;
        set
        {
            _selected = value;
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

    public ScriptType Type { get; }

    /// <summary>Executes this script.</summary>
    /// <remarks>Returns when this script has finished executing or was hung and has been terminated.</remarks>
    /// <inheritdoc cref="Host.ExecuteCode" path="/param"/>
    public void Execute(Action onHung, CancellationToken cancellationToken)
        => Host.ExecuteCode(Code, AppInfo.Settings.ScriptTimeout, onHung, cancellationToken);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
}