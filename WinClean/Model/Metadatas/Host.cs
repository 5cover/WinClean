using System.Windows.Media.Imaging;

namespace Scover.WinClean.Model.Metadatas;

public abstract record Host : ScriptLocalizedStringMetadata
{
    protected Host(LocalizedString name, LocalizedString description, BitmapSource? icon) : base(name, description) => Icon = icon;

    public abstract HostStartInfo CreateHostStartInfo(string code);

    public BitmapSource? Icon { get; }
}