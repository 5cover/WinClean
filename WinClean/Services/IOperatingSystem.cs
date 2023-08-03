namespace Scover.WinClean.Services;

/// <summary>Interacts with the operating system.</summary>
public interface IOperatingSystem
{
    public void OpenSytemPropertiesProtection();

    public void RestartForOSReconfig(bool force);
}