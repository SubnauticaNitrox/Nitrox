using System.Runtime.InteropServices;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows;

namespace Nitrox.Launcher.Models.Extensions;

public static class ProcessExExtensions
{
    public static void SetForegroundWindowAndRestore(this ProcessEx process)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }
        WindowsApi.BringProcessToFront(process.MainWindowHandle);
    }
}
