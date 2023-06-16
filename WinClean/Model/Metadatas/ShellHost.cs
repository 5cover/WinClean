using Semver;

using Vanara.PInvoke;

namespace Scover.WinClean.Model.Metadatas;

public sealed class ShellHost : Host
{
    private readonly string _commandLine;

    public ShellHost(LocalizedString name, LocalizedString description, SemVersionRange versions, (string filename, int index)? icon, string commandLine) : base(name, description, versions, icon) => _commandLine = commandLine;

    public override HostStartInfo CreateHostStartInfo(string code)
    {
        string commandLine = _commandLine.FormatWith(code);
        var args = Win32Error.ThrowLastErrorIf(Shell32.CommandLineToArgvW(Environment.ExpandEnvironmentVariables(commandLine)), args => !args.Any(), $"Script command line ({commandLine}) is invalid");
        return new(args[0], args.Length > 1 ? string.Join(' ', args[1..]) : "");
    }
}