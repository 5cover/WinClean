using System.ComponentModel;
using System.Runtime.CompilerServices;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>An immutable script deployed with the application.</summary>
public class Script : INotifyPropertyChanged
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
                  bool isDefault,
                  LocalizedString name,
                  LocalizedString description)
    {
        _category = category;
        _code = code;
        _host = host;
        _impact = impact;
        _recommendationLevel = recommendationLevel;
        IsDefault = isDefault;
        LocalizedDescription = description;
        LocalizedName = name;
    }

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

    public bool IsDefault { get; }

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

    /// <summary>Gets the localized description of this script.</summary>
    public LocalizedString LocalizedDescription { get; init; }

    /// <summary>Gets the localized name of this script.</summary>
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

    /// <summary>Executes the script.</summary>
    /// <inheritdoc cref="IHost.ExecuteCode(string, string, TimeSpan, HungScriptCallback, CancellationToken)"/>
    /// <remarks>Returns when the script has finished executing or has been killed.</remarks>
    public void Execute(HungScriptCallback keepRunningElseKill, CancellationToken cancellationToken)
        => Host.ExecuteCode(Code, Name, AppInfo.Settings.ScriptTimeout, keepRunningElseKill, cancellationToken);

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
}