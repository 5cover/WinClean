using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;

namespace Scover.WinClean.DataAccess;

/// <seealso href="https://stackoverflow.com/a/42733327/11718061"/>
public sealed class RestorePoint
{
    private readonly string _description;

    private readonly EventType _eventType;

    private readonly RestorePointType _type;

    /// <param name="description">The description to be displayed so the user can easily identify a restore point.</param>
    /// <param name="eventType">The type of event.</param>
    /// <param name="type">The type of restore point.</param>
    public RestorePoint(string description, EventType eventType, RestorePointType type)
    {
        _description = description;
        _eventType = eventType;
        _type = type;
    }

    /// <summary>Enables system restore for all eligible drives.</summary>
    public static void EnableSystemRestore()
    {
        // Some drives are non-eligible for system restore, but Enable-ComputerRestore will still enable the eligible ones. We
        // have to use Process for this because using the PowerShell API throws CommandNotFoundException for the
        // Enable-ComputerRestore cmdlet for some reason
        using Process powerShell = Helpers.StartPowerShell($"-Command Enable-ComputerRestore -Drive {string.Join(',', DriveInfo.GetDrives().Select(di => @$"""{di.Name}\"""))}").AssertNotNull();
        powerShell.WaitForExit();
    }

    /// <summary>Creates a restore point on the local system.</summary>
    /// <exception cref="ManagementException">Access denied.</exception>
    /// <exception cref="InvalidOperationException">System restore is disabled.</exception>
    public void Create()
    {
        // Cannot use the CheckPoint-Computer cmdlet for creating the restore point because it only allows a restore point to be
        // created every 24 hours.
        ManagementScope mScope = new(@"\\localhost\root\default");
        ManagementPath mPath = new("SystemRestore");
        ObjectGetOptions options = new();

        using ManagementClass mClass = new(mScope, mPath, options);
        using ManagementBaseObject parameters = mClass.GetMethodParameters("CreateRestorePoint");
        parameters["Description"] = _description;
        parameters["EventType"] = (int)_eventType;
        parameters["RestorePointType"] = (int)_type;

        try
        {
            _ = mClass.InvokeMethod("CreateRestorePoint", parameters, null!); // ! : InvokeMethodOptions is not needed.
        }
        // HRESULT -2147023838 = 0x80070422 : system restore is disabled
        catch (COMException e) when (e.HResult == -2147023838)
        {
            throw new InvalidOperationException("Cannot create system restore point because system protection is disabled.", e);
        }
    }
}