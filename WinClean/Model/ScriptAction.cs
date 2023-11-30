using System.Diagnostics;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model;

[DebuggerDisplay($"{{{nameof(Host)},nq}} program")]
public sealed class ScriptAction
{
    public ScriptAction(Host host, ISet<int> successExitCodes, string code, int order)
        => (Code, SuccessExitCodes, Host, Order) = (code, successExitCodes, host, order);

    public string Code { get; set; }
    public Host Host { get; set; }

    /// <summary>
    /// Gets or sets the order of execution. Actions with a lower order should be executed first.
    /// </summary>
    public int Order { get; set; }

    public ISet<int> SuccessExitCodes { get; }

    public HostStartInfo CreateHostStartInfo() => Host.CreateHostStartInfo(Code);
}