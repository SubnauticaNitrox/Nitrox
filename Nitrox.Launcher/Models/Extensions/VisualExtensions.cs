using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using NitroxModel.Platforms.OS.Windows;

namespace Nitrox.Launcher.Models.Extensions;

public static class VisualExtensions
{
    public static void ApplyOsWindowStyling(this Visual visual)
    {
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            return;
        }
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        IntPtr? windowHandle = TopLevel.GetTopLevel(visual)?.TryGetPlatformHandle()?.Handle;
        if (windowHandle.HasValue)
        {
            WindowsApi.EnableDefaultWindowAnimations(windowHandle.Value);
        }
    }
}
