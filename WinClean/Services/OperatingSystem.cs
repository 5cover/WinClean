using System.Diagnostics;

using Vanara.PInvoke;

using static Vanara.PInvoke.AdvApi32;
using static Vanara.PInvoke.SystemShutDownReason;

namespace Scover.WinClean.Services;

public sealed class OperatingSystem : IOperatingSystem
{
    public void OpenSystemPropertiesProtection()
        => Process.Start(new ProcessStartInfo
        {
            FileName = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%\\System32\\SystemPropertiesProtection.exe"),
            UseShellExecute = true, // This will show an UAC prompt if administrative privileges are required instead of throwing an exception
        });

    public void RestartForOSReconfig(bool force)
        => Win32Error.ThrowLastErrorIfFalse(InitiateSystemShutdownEx(null, null, 0, force, true,
            SHTDN_REASON_MAJOR_OPERATINGSYSTEM | SHTDN_REASON_MINOR_RECONFIG));
}