using CommunityToolkit.Mvvm.ComponentModel;

using ICSharpCode.AvalonEdit.Document;

using Scover.WinClean.Model;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionProgressViewModel : ObservableObject, IProgress<ProcessOutput>
{
    public TextDocument FullOutput { get; } = CreateDocument();
    public TextDocument StandardError { get; } = CreateDocument();
    public TextDocument StandardOutput { get; } = CreateDocument();

    public void Report(ProcessOutput value)
    {
        AppendLine(FullOutput, value.Text);
        OnPropertyChanged(nameof(FullOutput));

        (var document, var propName) = value.Kind switch
        {
            ProcessOutputKind.Error => (StandardError, nameof(StandardError)),
            ProcessOutputKind.Standard => (StandardOutput, nameof(StandardOutput)),
            _ => throw value.Kind.NewInvalidEnumArgumentException()
        };
        AppendLine(document, value.Text);
        OnPropertyChanged(propName);
    }

    private static void AppendLine(TextDocument document, string? text)
    {
        if (text is not null)
        {
            document.Insert(document.TextLength, text);
            document.Insert(document.TextLength, Environment.NewLine);
        }
    }

    private static TextDocument CreateDocument() => new() { UndoStack = { SizeLimit = 0 } };
}