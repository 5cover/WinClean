using System.ComponentModel;
using System.Runtime.CompilerServices;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic.Scripts;

/// <summary>A script that can be executed from a script host program.</summary>
public class Script : INotifyPropertyChanged, IUserVisible
{
    private RecommendationLevel _recommended;
    private bool _selected;

    /// <summary>Initializes a new instance of the <see cref="Script"/> class with the specified data.</summary>
    public Script(LocalizedString name,
                  LocalizedString description,
                  string code,
                  Category category,
                  RecommendationLevel recommended,
                  Impact impact,
                  Host host)
    {
        _recommended = recommended;
        Category = category;
        Code = code;
        LocalizedDescriptions = description;
        Host = host;
        Impact = impact;
        LocalizedNames = name;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Get or sets the category of this script.</summary>
    public Category Category { get; set; }

    /// <summary>Get or sets the code of this script.</summary>
    public string Code { get; set; }

    /// <summary>Gets or sets the description of this script</summary>
    /// <inheritdoc/>
    public string Description
    {
        get => LocalizedDescriptions.Get(CurrentUICulture);
        set => LocalizedDescriptions.Set(CurrentUICulture, value);
    }

    /// <summary>Gets or sets the host to use to run this script.</summary>
    public Host Host { get; set; }

    /// <summary>Gets or sets the impact of running this script on the system.</summary>
    public Impact Impact { get; set; }

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
    public void Execute(HungScriptCallback keepRunningOrKill, CancellationTokenSource? cancellationToken)
        => Host.ExecuteCode(Code, Name, AppInfo.Settings.ScriptTimeout, keepRunningOrKill, cancellationToken);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
}