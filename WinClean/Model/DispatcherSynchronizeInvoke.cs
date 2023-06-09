using System.ComponentModel;
using System.Windows.Threading;

namespace Scover.WinClean.Model;

public sealed class DispatcherSynchronizeInvoke : ISynchronizeInvoke
{
    private readonly Dispatcher _dispatcher;

    public DispatcherSynchronizeInvoke(Dispatcher dispatcher) => _dispatcher = dispatcher;

    public bool InvokeRequired => !_dispatcher.CheckAccess();

    public IAsyncResult BeginInvoke(Delegate method, object?[]? args) =>
           // Obtaining a DispatcherOperation instance and wrapping it with our proxy class
           new DispatcherAsyncResult(
           _dispatcher.BeginInvoke(method, DispatcherPriority.Normal, args));

    public object EndInvoke(IAsyncResult result)
    {
        DispatcherAsyncResult dispatcherResult = (DispatcherAsyncResult)result;
        _ = dispatcherResult.Operation.Wait();
        return dispatcherResult.Operation.Result;
    }

    public object Invoke(Delegate method, object?[]? args) => _dispatcher.Invoke(method, DispatcherPriority.Normal, args);

    // We also could use the DispatcherOperation.Task directly
    private sealed class DispatcherAsyncResult : IAsyncResult
    {
        private readonly IAsyncResult result;

        public DispatcherAsyncResult(DispatcherOperation operation)
        {
            Operation = operation;
            result = operation.Task;
        }

        public object? AsyncState => result.AsyncState;
        public WaitHandle AsyncWaitHandle => result.AsyncWaitHandle;
        public bool CompletedSynchronously => result.CompletedSynchronously;
        public bool IsCompleted => result.IsCompleted;
        public DispatcherOperation Operation { get; }
    }
}