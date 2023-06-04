using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Vanara.PInvoke;
using System.Globalization;
using Scover.WinClean.Model.Metadatas;

namespace Scover.WinClean.Model.Serialization.Xml;

public sealed class ScriptMetadataXmlDeserializer : IScriptMetadataDeserializer
{
    private readonly XmlDocument _doc = new();

    public IEnumerable<Category> GetCategories(Stream stream)
        => from category in GetLocalizable(stream, "Category")
           select new Category(category.name, category.description);

    public IEnumerable<Host> GetHosts(Stream stream)
        => from host in GetLocalizable(stream, "Host")
           let type = host.element.GetAttribute("Type")
           let iconElement = host.element.GetSingleChildOrDefault("Icon")
           let icon = iconElement is null ? null : GetHostIcon(iconElement.GetAttribute("Filename"), int.Parse(iconElement.GetAttribute("Index"), CultureInfo.InvariantCulture))
           select type switch
           {
               "Program" => (Host)new ProgramHost(host.name, host.description, icon, host.element.GetSingleChildText("Executable"), host.element.GetSingleChildText("Arguments"), host.element.GetSingleChildText("Extension")),
               "Shell" => new ShellHost(host.name, host.description, icon, host.element.GetSingleChildText("CommandLine")),
               _ => throw new InvalidDataException($"Unknwon host type '{type}'"),
           };

    public IEnumerable<Impact> GetImpacts(Stream stream)
        => from impact in GetLocalizable(stream, "Impact")
           select new Impact(impact.name, impact.description);

    public IEnumerable<SafetyLevel> GetSafetyLevels(Stream stream)
        => from safetyLevel in GetLocalizable(stream, "SafetyLevel")
           select new SafetyLevel(safetyLevel.name, safetyLevel.description, (Color)ColorConverter.ConvertFromString(safetyLevel.element.GetAttribute("Color")));

    private IEnumerable<XmlElement> GetElements(Stream s, string name)
    {
        _doc.Load(s);
        return _doc.GetElementsByTagName(name).Cast<XmlElement>();
    }

    private IEnumerable<(XmlElement element, LocalizedString name, LocalizedString description)> GetLocalizable(Stream stream, string elementName)
    {
        foreach (var element in GetElements(stream, elementName))
        {
            LocalizedString name = new(), description = new();
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name)
                {
                    case "Name":
                        name.SetFromXml(child);
                        break;

                    case "Description":
                        description.SetFromXml(child);
                        break;
                }
            }
            yield return (element, name, description);
        }
    }

    private static BitmapSource? GetHostIcon(string filename, int index)
    {
        _ = Shell32.ExtractIconEx(filename, index, 1, out var _, out var smallIcon);
        return smallIcon[0].IsInvalid ? null : ((HICON)smallIcon[0]).ToBitmapSource();
    }
}