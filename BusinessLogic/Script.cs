using Scover.WinClean.BusinessLogic.ScriptExecution;
using Scover.WinClean.DataAccess;

using System.ComponentModel;
using System.Runtime.CompilerServices;

using static System.Globalization.CultureInfo;

namespace Scover.WinClean.BusinessLogic;

/// <summary>A script that can be executed from a script host program.</summary>
public class Script : INotifyPropertyChanged
{
    private RecommendationLevel _recommended;
    private bool _selected;

    /// <summary>Initializes a new instance of the <see cref="Script"/> class with the specified data.</summary>
    public Script(RecommendationLevel recommended,
                  Category category,
                  string code,
                  Localized<string> localizedDescriptions,
                  ScriptHost host,
                  string filename,
                  Impact impact,
                  Localized<string> localizedNames)
    {
        _recommended = recommended;
        Category = category;
        Code = code;
        LocalizedDescriptions = localizedDescriptions;
        Host = host;
        Filename = filename;
        Impact = impact;
        LocalizedNames = localizedNames;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Get or sets the <see cref="BusinessLogic.Category"/> object used to classify this script.</summary>
    public Category Category { get; set; }

    /// <summary>Get or sets this script's code.</summary>
    public string Code { get; set; }

    /// <summary>Gets or sets the description of this script</summary>
    /// <value>A free-form description of this script, localied to <see cref="CurrentUICulture"/>.</value>
    public string Description
    {
        get => LocalizedDescriptions.Get(CurrentUICulture);
        set => LocalizedDescriptions.Set(CurrentUICulture, value);
    }

    /// <summary>Gets the name of the file this script was deserialized from.</summary>
    public string Filename { get; }

    /// <summary>Gets or sets the host to use to run this script.</summary>
    public ScriptHost Host { get; set; }

    /// <summary>Gets or sets the impact of running this script on the system.</summary>
    public Impact Impact { get; set; }

    /// <summary>Gets the localized descriptions of this script.</summary>
    /// <value>The descriptions of this script in all available languages.</value>
    public Localized<string> LocalizedDescriptions { get; }

    /// <summary>Gets the localized names of this script.</summary>
    /// <value>The names of this script in all available languages.</value>
    public Localized<string> LocalizedNames { get; }

    /// <summary>Gets or sets the name of this script</summary>
    /// <value>A short, human-redable name for this script, localized to <see cref="CurrentUICulture"/>.</value>
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

    /// <summary>Gets or sets whether this script has been selected for execution by the user.</summary>
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Executes the script.</summary>
    /// <inheritdoc cref="ScriptHost.ExecuteCode(string, string, TimeSpan, Func{string, bool}, Func{Exception, FileSystemInfo, FSVerb, bool})" path="/param"/>
    /// <inheritdoc cref="ScriptHost.ExecuteCode(string, string, TimeSpan, Func{string, bool}, Func{Exception, FileSystemInfo, FSVerb, bool})" path="/exception"/>
    public void Execute(TimeSpan timeout, Func<string, bool> promptEndTaskOnHung, Func<Exception, FileSystemInfo, FSVerb, bool> promptRetryOnFSError)
        => Host.ExecuteCode(Code, Name, timeout, promptEndTaskOnHung, promptRetryOnFSError);

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
}