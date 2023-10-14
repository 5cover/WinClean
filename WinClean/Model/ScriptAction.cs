using System.Diagnostics;

using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model;

[DebuggerDisplay($"{{{nameof(Host)},nq}} program")]
public sealed class ScriptAction : IEquatable<ScriptAction?>
{
    public ScriptAction(Host host, IEnumerable<int> successsExitCodes, string code)
        => (Code, SuccessExitCodes, Host) = (code, successsExitCodes, host);

    public string Code { get; set; }
    public IEnumerable<int> SuccessExitCodes { get; }
    public Host Host { get; set; }

    public HostStartInfo CreateHostStartInfo() => Host.CreateHostStartInfo(Code);

    public override bool Equals(object? obj) => Equals(obj as ScriptAction);

    public bool Equals(ScriptAction? other) => other is not null && Code == other.Code && EqualityComparer<Host>.Default.Equals(Host, other.Host);

    public override int GetHashCode() => HashCode.Combine(Code, Host);
}