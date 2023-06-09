using System.Text;

using Scover.WinClean.Model;
using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;

using Semver;

namespace Tests;

public sealed partial class ScriptMetadataXmlDeserializerTests
{
    private abstract class TestHost<T> : TestSerializable<T>
    {
        protected TestHost(string type,
                           LocalizedString name,
                           LocalizedString description,
                           SemVersionRange versions,
                           (string, int)? icon,
                           StringBuilder next,
                           T value) : base(
            xml: new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>").Append(Element(ElementFor.Host,
                     name.FormatXml(ElementFor.Name)
                     .Append(description.FormatXml(ElementFor.Description))
                     .Append(Element(ElementFor.VersionRange, versions.ToString()))
                     .Append(icon is (string filename, int index) ? Element(ElementFor.Icon, (ElementFor.Filename, filename), (ElementFor.IconIndex, index.ToString())) : null)
                     .Append(next), (ElementFor.Type, type))),
            value: value)
        {
        }
    }

    private sealed class TestProgramHost : TestHost<ProgramHost>
    {
        public TestProgramHost(LocalizedString name,
                               LocalizedString description,
                               SemVersionRange versions,
                               (string, int)? icon,
                               string executable,
                               string arguments,
                               string extension) : base(ElementFor.ProgramHost, name, description, versions, icon,
            next: Element(ElementFor.Executable, executable).Append(Element(ElementFor.Arguments, arguments)).Append(Element(ElementFor.Extension, extension)),
            value: new ProgramHost(name, description, versions, icon, executable, arguments, extension))
        {
        }
    }

    private sealed class TestShellHost : TestHost<ShellHost>
    {
        public TestShellHost(LocalizedString name,
                             LocalizedString description,
                             SemVersionRange versions,
                             (string, int)? icon,
                             string commandLine) : base(ElementFor.ShellHost, name, description, versions, icon,
            next: Element(ElementFor.CommandLine, commandLine),
            value: new ShellHost(name, description, versions, icon, commandLine))
        {
        }
    }
}