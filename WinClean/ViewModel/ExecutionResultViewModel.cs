using ExecutionResult = Scover.WinClean.Model.ExecutionResult;

namespace Scover.WinClean.ViewModel;

public sealed class ExecutionResultViewModel
{
    private readonly ExecutionResult _model;

    public ExecutionResultViewModel(ExecutionResult model) => _model = model;

    public TimeSpan ExecutionTime => _model.ExecutionTime;
    public int ExitCode => _model.ExitCode;
    public string FormattedExecutionTime => ExecutionTime.HumanizeToMilliseconds();
    public bool Succeeded => _model.Succeeded;
}