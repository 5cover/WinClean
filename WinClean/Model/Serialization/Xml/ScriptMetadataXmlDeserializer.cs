using System.Globalization;
using System.Windows.Media;
using System.Xml;

using Scover.WinClean.Model.Metadatas;
using Scover.WinClean.Resources;
using Scover.WinClean.Services;

using Semver;

namespace Scover.WinClean.Model.Serialization.Xml;

public sealed class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    public IEnumerable<Category> GetCategories(Stream stream)
        => from category in GetLocalizedElements(stream, NameFor.Category)
           select new Category(category.name, category.description, GetOrder(category.element));

    public IEnumerable<Host> GetHosts(Stream stream)
        => from host in GetLocalizedElements(stream, NameFor.Host)

           let versions = host.element.GetSingleChildTextOrDefault(NameFor.VersionRange) is { } versionsString
               ? SemVersionRange.Parse(versionsString)
               : ServiceProvider.Get<ISettings>().DefaultHostVersions

           let icon = host.element.GetSingleChildOrDefault(NameFor.Icon) is { } iconElement
               ? (iconElement.GetAttribute(NameFor.Filename), int.Parse(iconElement.GetAttribute(NameFor.IconIndex), CultureInfo.InvariantCulture))
               : ((string, int)?)null

           let type = host.element.GetAttribute(NameFor.Type)
           select type switch
           {
               _ when type == NameFor.ProgramHost => (Host)new ProgramHost(host.name, host.description, versions, icon,
                   host.element.GetSingleChildText(NameFor.Executable),
                   host.element.GetSingleChildText(NameFor.Arguments),
                   host.element.GetSingleChildText(NameFor.Extension)),
               _ when type == NameFor.ShellHost => new ShellHost(host.name, host.description, versions, icon,
                   host.element.GetSingleChildText(NameFor.CommandLine)),
               _ => throw new DeserializationException(nameof(Host), null, new InvalidDataException(ExceptionMessages.UnknownHostType.FormatWith(type))),
           };

    public IEnumerable<Impact> GetImpacts(Stream stream)
        => from impact in GetLocalizedElements(stream, NameFor.Impact)
           select new Impact(impact.name, impact.description);

    public IEnumerable<SafetyLevel> GetSafetyLevels(Stream stream)
        => from safetyLevel in GetLocalizedElements(stream, NameFor.SafetyLevel)
           select new SafetyLevel(safetyLevel.name, safetyLevel.description, GetOrder(safetyLevel.element), (Color)ColorConverter.ConvertFromString(safetyLevel.element.GetAttribute(NameFor.Color)));

    private static IEnumerable<XmlElement> GetElements(Stream s, string name)
    {
        XmlDocument d = new();
        d.Load(s);
        return d.GetElementsByTagName(name).Cast<XmlElement>();
    }

    private static IEnumerable<(XmlElement element, LocalizedString name, LocalizedString description)> GetLocalizedElements(Stream stream, string elementName)
    {
        foreach (var element in GetElements(stream, elementName))
        {
            LocalizedString name = new(), description = new();

            foreach (XmlElement child in element.ChildNodes)
            {
                (child.Name switch
                {
                    _ when child.Name == NameFor.Name => name,
                    _ when child.Name == NameFor.Description => description,
                    _ => null
                })?.SetFromXml(child);
            }
            yield return (element, name, description);
        }
    }

    private static int GetOrder(XmlElement element) => int.Parse(element.GetAttribute(NameFor.Order));
}