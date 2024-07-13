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
        // Only apply style if window can resize. Otherwise, (on Windows) it will force resizing and might look ugly if not accounted for in UI.
        if (visual.GetWindow() is not { CanResize: true } window)
        {
            return;
        }

        IntPtr? windowHandle = window.TryGetPlatformHandle()?.Handle;
        if (windowHandle.HasValue)
        {
            WindowsApi.EnableDefaultWindowAnimations(windowHandle.Value);
        }
    }

    public static Window GetWindow(this Visual visual) => TopLevel.GetTopLevel(visual) as Window;
}
