using Scover.WinClean.Operational;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Scover.WinClean.Logic;

/// <summary>A script that can be executed from a script host program.</summary>
public class Script : INotifyPropertyChanged
{
    private Advised _advised;
    private string _name;
    private bool _selected;

    /// <summary>Initializes a new instance of the <see cref="Script"/> class with the specified data.</summary>
    public Script(Advised advised, Category category, string code, string description, ScriptHost host, string filename, Impact impact, string name)
    {
        _advised = advised;
        Category = category;
        Code = code;
        Description = description.Trim();
        Host = host;
        Filename = filename;
        Impact = impact;
        _name = name.Trim();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Advised Advised
    {
        get => _advised;
        set
        {
            _advised = value;
            OnPropertyChanged();
        }
    }

    public Category Category { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public string Filename { get; }
    public ScriptHost Host { get; set; }
    public Impact Impact { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

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