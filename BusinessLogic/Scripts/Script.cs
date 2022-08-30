using System.ComponentModel;
using System.Runtime.CompilerServices;

using Scover.WinClean.BusinessLogic.Scripts.Hosts;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A script that can be executed from a script host program.</summary>
public class Script : INotifyPropertyChanged, IScriptData
{
    private Category _category;
    private string _code;
    private TimeSpan _executionTime;
    private IHost _host;
    private Impact _impact;
    private RecommendationLevel _recommended;
    private bool _selected;

    /// <summary>Initializes a new instance of the <see cref="Script"/> class with the specified data.</summary>
    public Script(Category category,
                  string code,
                  TimeSpan executionTime,
                  IHost host,
                  Impact impact,
                  RecommendationLevel recommended,
                  LocalizedString descriptions,
                  LocalizedString names)
    {
        _category = category;
        _code = code;
        _executionTime = executionTime;
        _host = host;
        _impact = impact;
        _recommended = recommended;
        LocalizedDescriptions = descriptions;
        LocalizedNames = names;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Get or sets the category of this script.</summary>
    public Category Category
    {
        get => _category; set
        {
            _category = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Get or sets the code of this script.</summary>
    public string Code
    {
        get => _code;
        set
        {
            _code = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the description of this script</summary>
    /// <inheritdoc/>
    public string Description
    {
        get => LocalizedDescriptions.Get(CurrentUICulture);
        set => LocalizedDescriptions.Set(CurrentUICulture, value);
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

    /// <summary>Gets or sets the host to use to run this script.</summary>
    public IHost Host
    {
        get => _host;
        set
        {
            _host = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the impact of running this script on the system.</summary>
    public Impact Impact
    {
        get => _impact;
        set
        {
            _impact = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the invariant name of this instance</summary>
    /// <inheritdoc/>
    public string InvariantName
    {
        get => LocalizedNames.Get(InvariantCulture);
        set => LocalizedNames.Set(InvariantCulture, value);
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

    /// <summary>Gets the localized descriptions of this script.</summary>
    /// <value>The descriptions of this script in all available languages.</value>
    public LocalizedString LocalizedDescriptions { get; }

    /// <summary>Gets the localized names of this script.</summary>
    /// <value>The names of this script in all available languages.</value>
    public LocalizedString LocalizedNames { get; }

    /// <summary>Gets or sets the name of this instance</summary>
    /// <inheritdoc/>
    public string Name
    {
        get => LocalizedNames.Get(CurrentUICulture);
        set
        {
            LocalizedNames.Set(CurrentUICulture, value);
            OnPropertyChanged();
        }
    }

    /// <summary>Gets or sets the level of recommendation of this script.</summary>
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
    /// <inheritdoc cref="Host.ExecuteCode"/>
    /// <remarks>Returns when the script has finished executing or has been killed.</remarks>
    public void Execute(HungScriptCallback keepRunningOrKill, CancellationToken cancellationToken)
        => Host.ExecuteCode(Code, Name, AppInfo.Settings.ScriptTimeout, keepRunningOrKill, cancellationToken);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
}