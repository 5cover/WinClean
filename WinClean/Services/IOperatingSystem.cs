namespace Scover.WinClean.Services;

/// <summary>Interacts with the operating system.</summary>
public interface IOperatingSystem
{
    public IEnumerable<DriveInfo> SystemRestoreEligibleDrives { get; }

    /// <summary>Creates a restore point on the local system.</summary>
    /// <exception cref="InvalidOperationException">System restore is disabled.</exception>
    void CreateRestorePoint(string description, RestorePointType type, RestorePointEventType eventType);

    public void DisableSystemRestore(DriveInfo drive);

    public void EnableSystemRestore(DriveInfo drive);

    public void RestartForOSReconfig(bool force);
}