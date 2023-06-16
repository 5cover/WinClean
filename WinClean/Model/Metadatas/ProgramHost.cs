using Semver;

namespace Scover.WinClean.Model.Metadatas;

public sealed class ProgramHost : Host
{
    private readonly string _arguments, _executable, _extension;

    public ProgramHost(LocalizedString name, LocalizedString description, SemVersionRange versions, (string filename, int index)? icon, string executable, string arguments, string extension) : base(name, description, versions, icon)
        => (_executable, _arguments, _extension) = (Environment.ExpandEnvironmentVariables(executable), Environment.ExpandEnvironmentVariables(arguments), extension);

    public override HostStartInfo CreateHostStartInfo(string code) => new HostTempFileStartInfo(_executable, _arguments, code, _extension);
}