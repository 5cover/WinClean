using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;

using Scover.WinClean.Model;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionProgressViewModel : ObservableObject, IProgress<ProcessOutput>
{
    public string FullOutput => _fullOutput.ToString();
    public string StandardError => _standardError.ToString();
    public string StandardOutput => _standardOutput.ToString();

    private readonly StringBuilder _fullOutput = new(), _standardError = new(), _standardOutput = new();

    public void Report(ProcessOutput value)
    {
        _ = _fullOutput.AppendLine(value.Text);
        OnPropertyChanged(nameof(FullOutput));

        (var builder, var propName) = value.Kind switch
        {
            ProcessOutputKind.Error => (_standardError, nameof(StandardError)),
            ProcessOutputKind.Standard => (_standardOutput, nameof(StandardOutput)),
            _ => throw value.Kind.NewInvalidEnumArgumentException()
        };
        _ = builder.AppendLine(value.Text);
        OnPropertyChanged(propName);
    }
}