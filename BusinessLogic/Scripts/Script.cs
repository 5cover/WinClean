using System.ComponentModel;
using System.Runtime.CompilerServices;

using Scover.WinClean.BusinessLogic.Scripts.Hosts;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>An immutable script deployed with the application.</summary>
public class Script : INotifyPropertyChanged, IScriptData
{
    private Category _category;
    private string _code;
    private TimeSpan _executionTime;
    private IHost _host;
    private Impact _impact;
    private RecommendationLevel _recommended;
    private bool _selected;

    public Script(Category category,
                  string code,
                  TimeSpan executionTime,
                  IHost host,
                  Impact impact,
                  RecommendationLevel recommended,
                  bool isDefault,
                  LocalizedString localizedDescriptions,
                  LocalizedString localizedNames)
    {
        _category = category;
        _code = code;
        _executionTime = executionTime;
        _host = host;
        _impact = impact;
        _recommended = recommended;
        IsDefault = isDefault;
        LocalizedDescriptions = localizedDescriptions;
        LocalizedNames = localizedNames;
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
        get => LocalizedDescriptions.Get(CurrentUICulture);
        set
        {
            LocalizedDescriptions.Set(CurrentUICulture, value);
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets an approximate of the time it would take to execute this script.</summary>
    /// <value>
    /// The estimated execution time of this script, or <see cref="AppInfo.Settings.ScriptTimeout"/> if the estimate is not available.
    /// </value>
    public TimeSpan ExecutionTime
    {
        get => _executionTime;
        set
        {
            _executionTime = value;
            OnPropertyChanged();
        }
    }

    public IHost Host
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
        get => LocalizedNames.Get(InvariantCulture);
        set
        {
            LocalizedNames.Set(InvariantCulture, value);
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

    /// <summary>Gets the localized descriptions of this script.</summary>
    /// <value>The descriptions of this script in all available languages.</value>
    public LocalizedString LocalizedDescriptions { get; init; }

    /// <summary>Gets the localized names of this script.</summary>
    /// <value>The names of this script in all available languages.</value>
    public LocalizedString LocalizedNames { get; init; }

    public string Name
    {
        get => LocalizedNames.Get(CurrentUICulture);
        set
        {
            LocalizedNames.Set(CurrentUICulture, value);
            OnPropertyChanged();
        }
    }

    public RecommendationLevel Recommended
    {
        get => _recommended;
        set
        {
            _recommended = value;
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