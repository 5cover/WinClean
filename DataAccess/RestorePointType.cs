namespace Scover.WinClean.DataAccess;

public enum RestorePointType
{
    /// <summary>An application has been installed.</summary>
    ApplicationInstall = 0x0,

    /// <summary>An application has been uninstalled.</summary>
    ApplicationUninstall = 0x1,

    /// <summary>
    /// An application needs to delete the restore point it created. For example, an application would use this flag when a user
    /// cancels an installation.
    /// </summary>
    CancelledOperation = 0xd,

    /// <summary>A device driver has been installed.</summary>
    DeviceDriverInstall = 0xa,

    /// <summary>An application has had features added or removed.</summary>
    ModifySettings = 0xc
}