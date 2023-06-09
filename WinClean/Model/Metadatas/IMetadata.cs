using System.Globalization;

namespace Scover.WinClean.Model.Metadatas;

public interface IMetadata : IComparable
{
    /// <summary>Gets the description for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    string Description { get; }

    /// <summary>Gets the description for <see cref="CultureInfo.InvariantCulture"/>.</summary>
    string InvariantDescription { get; }

    /// <summary>Gets the name for <see cref="CultureInfo.InvariantCulture"/>.</summary>
    string InvariantName { get; }

    /// <summary>Gets the name for <see cref="CultureInfo.CurrentUICulture"/>.</summary>
    string Name { get; }
}