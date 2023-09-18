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
        => from category in GetLocalizable(stream, ElementFor.Category)
           select new Category(category.name, category.description, GetOrder(category.element));

    public IEnumerable<Host> GetHosts(Stream stream)
        => from host in GetLocalizable(stream, ElementFor.Host)

           let versions = host.element.GetSingleChildTextOrDefault(ElementFor.VersionRange) is { } versionsString
               ? SemVersionRange.Parse(versionsString)
               : ServiceProvider.Get<ISettings>().DefaultHostVersions

           let icon = host.element.GetSingleChildOrDefault(ElementFor.Icon) is { } iconElement
               ? (iconElement.GetAttribute(ElementFor.Filename), int.Parse(iconElement.GetAttribute(ElementFor.IconIndex), CultureInfo.InvariantCulture))
               : ((string, int)?)null

           let type = host.element.GetAttribute(ElementFor.Type)
           select type switch
           {
               _ when type == ElementFor.ProgramHost => (Host)new ProgramHost(host.name, host.description, versions, icon,
                   host.element.GetSingleChildText(ElementFor.Executable),
                   host.element.GetSingleChildText(ElementFor.Arguments),
                   host.element.GetSingleChildText(ElementFor.Extension)),
               _ when type == ElementFor.ShellHost => new ShellHost(host.name, host.description, versions, icon,
                   host.element.GetSingleChildText(ElementFor.CommandLine)),
               _ => throw new DeserializationException("Host", null, new InvalidDataException(ExceptionMessages.UnknownHostType.FormatWith(type))),
           };

    public IEnumerable<Impact> GetImpacts(Stream stream)
        => from impact in GetLocalizable(stream, ElementFor.Impact)
           select new Impact(impact.name, impact.description);

    public IEnumerable<SafetyLevel> GetSafetyLevels(Stream stream)
        => from safetyLevel in GetLocalizable(stream, ElementFor.SafetyLevel)
           select new SafetyLevel(safetyLevel.name, safetyLevel.description, GetOrder(safetyLevel.element), (Color)ColorConverter.ConvertFromString(safetyLevel.element.GetAttribute(ElementFor.Color)));

    private static IEnumerable<XmlElement> GetElements(Stream s, string name)
    {
        XmlDocument d = new();
        d.Load(s);
        return d.GetElementsByTagName(name).Cast<XmlElement>();
    }

    private static IEnumerable<(XmlElement element, LocalizedString name, LocalizedString description)> GetLocalizable(Stream stream, string elementName)
    {
        foreach (var element in GetElements(stream, elementName))
        {
            LocalizedString name = new(), description = new();

            foreach (XmlElement child in element.ChildNodes)
            {
                (child.Name switch
                {
                    _ when child.Name == ElementFor.Name => name,
                    _ when child.Name == ElementFor.Description => description,
                    _ => null
                })?.SetFromXml(child);
            }
            yield return (element, name, description);
        }
    }

    private static int GetOrder(XmlElement element) => int.Parse(element.GetAttribute(ElementFor.Order));
}