using System.Diagnostics;
using System.Runtime.InteropServices;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel.Platforms.OS.Windows;

namespace Nitrox.Launcher.Models.Extensions;

public static class ProcessExExtensions
{
    public static void SetForegroundWindowAndRestore(this ProcessEx process)
    {
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            return;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsApi.BringProcessToFront(process.MainWindowHandle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // xdotool sends an XEvent to X11 window manager on Linux systems. 
            string command = $"xdotool windowactivate $(xdotool search --pid {process.Id} --onlyvisible --desktop '$(xdotool get_desktop)' --name 'nitrox launcher')";
            using Process proc = Process.Start(new ProcessStartInfo
            {
                FileName = "sh",
                ArgumentList = { "-c", command },
            });

            // TODO: Support "bring to front" on Wayland window manager.
        }
    }
}
