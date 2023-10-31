using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Model;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionProgressViewModel : ObservableObject, IProgress<ProcessOutput>
{
    private readonly StringBuilder _fullOutput = new(), _standardError = new(), _standardOutput = new();

    // StringBuilder is not thread-safe : locks are necessary

    public string FullOutput
    {
        get
        {
            lock (_fullOutput)
            {
                return _fullOutput.ToString();
            }
        }
    }

    public string StandardError
    {
        get
        {
            lock (_standardError)
            {
                return _standardError.ToString();
            }
        }
    }

    public string StandardOutput
    {
        get
        {
            lock (_standardOutput)
            {
                return _standardOutput.ToString();
            }
        }
    }

    public void Report(ProcessOutput value)
    {
        lock (_fullOutput)
        {
            _ = _fullOutput.AppendLine(value.Text);
        }

        OnPropertyChanged(nameof(FullOutput));

        (var builder, var propName) = value.Kind switch
        {
            ProcessOutputKind.Error => (_standardError, nameof(StandardError)),
            ProcessOutputKind.Standard => (_standardOutput, nameof(StandardOutput)),
            _ => throw value.Kind.NewInvalidEnumArgumentException()
        };

        lock (builder)
        {
            _ = builder.AppendLine(value.Text);
        }

        OnPropertyChanged(propName);
    }
}