namespace Scover.WinClean.Services;

/// <summary>Interacts with the operating system.</summary>
public interface IOperatingSystem
{
    public IEnumerable<DriveInfo> EligibleDrives { get; }

    void CreateRestorePoint(string description, RestorePointEventType eventType, RestorePointType type);

    public void DisableSystemRestore(DriveInfo drive);

    public void EnableSystemRestore(DriveInfo drive);

    public void RestartForOSReconfig(bool force);
}