using System.Diagnostics;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model;

[DebuggerDisplay($"{{{nameof(Host)},nq}} program")]
public sealed class ScriptAction : IEquatable<ScriptAction?>
{
    /// <param name="successsExitCodes">Success exit codes. The collection is cloned.</param>
    public ScriptAction(Host host, ISet<int> successsExitCodes, string code, int order)
        => (Code, SuccessExitCodes, Host, Order) = (code, successsExitCodes, host, order);

    public string Code { get; set; }
    public ISet<int> SuccessExitCodes { get; }

    /// <summary>
    /// Gets or sets the order of execution. Actions with a lower order should be executed first.
    /// </summary>
    public int Order { get; set; }

    public Host Host { get; set; }

    public HostStartInfo CreateHostStartInfo() => Host.CreateHostStartInfo(Code);

    public override bool Equals(object? obj) => Equals(obj as ScriptAction);

    public bool Equals(ScriptAction? other) => other is not null && Code == other.Code && EqualityComparer<Host>.Default.Equals(Host, other.Host);

    public override int GetHashCode() => HashCode.Combine(Code, Host);
}