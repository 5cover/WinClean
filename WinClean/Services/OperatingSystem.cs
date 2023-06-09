using System.Management;
using System.Runtime.InteropServices;

using Vanara.PInvoke;

using static Vanara.PInvoke.AdvApi32;
using static Vanara.PInvoke.SystemShutDownReason;

namespace Scover.WinClean.Services;

public sealed class OperatingSystem : IOperatingSystem
{
    public IEnumerable<DriveInfo> EligibleDrives
    {
        get
        {
            using ManagementObjectSearcher searcher = new(@"\\.\root\CIMV2", "SELECT Dependent FROM Win32_ShadowVolumeSupport");

            foreach (var driveLetter in searcher.Get().Cast<ManagementObject>()
                .Select(shadowVolumeSupport => (string)new ManagementObject((string)shadowVolumeSupport["Dependent"])["DriveLetter"])
                .Where(driveLetter => !string.IsNullOrEmpty(driveLetter)))
            {
                yield return new(driveLetter);
            }
        }
    }

    public void CreateRestorePoint(string description, RestorePointEventType eventType, RestorePointType type)
    {
        using ManagementClass systemRestore = new(@"\\localhost\root\default", "SystemRestore", new());
        try
        {
            _ = systemRestore.InvokeMethod("CreateRestorePoint", new object[] { description, eventType, type });
        }
        catch (COMException e) when (e.HResult == unchecked((int)0x80070422)) // system restore is disabled
        {
            throw new InvalidOperationException("Cannot create system restore point because system restore is disabled.", e);
        }
    }

    public void DisableSystemRestore(DriveInfo drive)
    {
        using ManagementClass systemRestore = new(@"\\localhost\root\default", "SystemRestore", new());
        _ = systemRestore.InvokeMethod("Enable", new[] { drive.Name });
    }

    public void EnableSystemRestore(DriveInfo drive)
    {
        using ManagementClass systemRestore = new(@"\\localhost\root\default", "SystemRestore", new());
        _ = systemRestore.InvokeMethod("Disable", new[] { drive.Name });
    }

    public void RestartForOSReconfig(bool force)
        => Win32Error.ThrowLastErrorIfFalse(InitiateSystemShutdownEx(null, null, 0, force, true,
            SHTDN_REASON_MAJOR_OPERATINGSYSTEM | SHTDN_REASON_MINOR_RECONFIG));
}