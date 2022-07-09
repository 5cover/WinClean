using Scover.WinClean.DataAccess;

using System.Globalization;
using System.Windows.Media;
using System.Xml;

namespace Scover.WinClean.BusinessLogic;

public class RecommendationLevel
{
    private readonly Localized<string> _description;

    private readonly Localized<string> _name;

    private RecommendationLevel(Localized<string> name, Localized<string> description, Color color)
    {
        _name = name;
        _description = description;
        Color = color;
    }

    public static IEnumerable<RecommendationLevel> Values { get; } = LoadRecommendationLevels();
    public Color Color { get; }
    public string Description => _description.Get(CultureInfo.CurrentUICulture);
    public string Name => _name.Get(CultureInfo.CurrentUICulture);

    /// <summary>Gets the <see cref="RecommendationLevel"/> matching the specified name in an invariant culture.</summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> does not match to any <see cref="RecommendationLevel"/> name. --OR-- <paramref name="culture"/>
    /// was not found.
    /// </exception>
    public static RecommendationLevel FromName(string name) => Values.SingleOrDefault(value => value._name.Get(CultureInfo.InvariantCulture) == name)
           ?? throw new ArgumentException(Resources.DevException.InvalidTypeProp.FormatWithInvariant(nameof(RecommendationLevel), nameof(_name)), nameof(name));

    public override string ToString() => _name.Get(CultureInfo.InvariantCulture);

    private static IEnumerable<RecommendationLevel> LoadRecommendationLevels()
    {
        List<RecommendationLevel> recommendationLevels = new();

        XmlDocument doc = new();
        doc.Load(App.GetContentStream("RecommendationLevels.xml"));

        foreach (XmlElement element in doc.GetElementsByTagName("RecommendationLevel"))
        {
            Localized<string> name = new();
            Localized<string> description = new();

            foreach (XmlElement child in element.ChildNodes)
            {
                if (child.Name == "Name")
                {
                    name.SetFromXml(child);
                }
                else if (child.Name == "Description")
                {
                    description.SetFromXml(child);
                }
            }
            recommendationLevels.Add(new(name, description, (Color)ColorConverter.ConvertFromString(element.GetAttribute("Color"))));
        }

        return recommendationLevels;
    }
}