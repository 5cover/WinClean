using System.Windows.Media.Imaging;

using Semver;

using Vanara.PInvoke;

namespace Scover.WinClean.Model.Metadatas;

public abstract class Host : Metadata
{
    private readonly (string filename, int index)? _icon;

    protected Host(LocalizedString name, LocalizedString description, SemVersionRange versions, (string filename, int index)? icon)
        : base(new LocalizedStringTextProvider(name, description))
        => (Versions, _icon) = (versions, icon);

    /// <summary>Fetches the icon.</summary>
    /// <value>
    /// The icon of this host, or <see langword="null"/> if no icon is defined or if it couldn't be fetched.
    /// </value>
    public BitmapSource? Icon
    {
        get
        {
            if (_icon is not (string filename, int index))
            {
                return null;
            }
            _ = Shell32.ExtractIconEx(filename, index, 1, out var _, out var smallIcon);
            return smallIcon[0].IsInvalid ? null : ((HICON)smallIcon[0]).ToBitmapSource();
        }
    }

    /// <summary>The windows versions this host supports.</summary>
    public SemVersionRange Versions { get; }

    public abstract HostStartInfo CreateHostStartInfo(string code);
}