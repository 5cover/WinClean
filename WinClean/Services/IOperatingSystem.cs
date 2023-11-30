namespace Scover.WinClean.Services;

/// <summary>Interacts with the operating system.</summary>
public interface IOperatingSystem
{
    public void OpenSystemPropertiesProtection();

    public void RestartForOSReconfig(bool force);
}