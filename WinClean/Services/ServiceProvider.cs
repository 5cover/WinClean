using Jab;

namespace Scover.WinClean.Services;

[ServiceProvider]
[Singleton(typeof(IDialogCreator), typeof(DialogCreator))]
[Singleton(typeof(IApplicationInfo), typeof(ApplicationInfo))]
[Singleton(typeof(IViewResolver), typeof(ViewResolver))]
[Singleton(typeof(IOperatingSystem), typeof(OperatingSystem))]
[Singleton(typeof(ISettings), typeof(Settings))]
[Singleton(typeof(IThemeProvider), typeof(ThemeProvider))]
[Singleton(typeof(IMetadatasProvider), typeof(MetadatasProvider))]
[Singleton(typeof(IScriptStorage), typeof(ScriptStorage))]
public sealed partial class ServiceProvider
{
    private static readonly ServiceProvider _instance = new();

    private ServiceProvider()
    {
    }

    public static T Get<T>() => _instance.GetService<T>();
}