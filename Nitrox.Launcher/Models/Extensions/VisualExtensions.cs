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
        TopLevel topLevel = TopLevel.GetTopLevel(visual);
        // Only apply style if window can resize. Otherwise, (on Windows) it will force resizing and might look ugly if not accounted for in UI.
        if (topLevel is Window { CanResize: false })
        {
            return;
        }

        IntPtr? windowHandle = topLevel?.TryGetPlatformHandle()?.Handle;
        if (windowHandle.HasValue)
        {
            WindowsApi.EnableDefaultWindowAnimations(windowHandle.Value);
        }
    }
}
