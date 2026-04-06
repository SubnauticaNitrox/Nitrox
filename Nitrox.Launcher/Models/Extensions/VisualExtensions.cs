using System;
using Avalonia;
using Avalonia.Controls;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.Platforms.OS.Windows;

namespace Nitrox.Launcher.Models.Extensions;

public static class VisualExtensions
{
    public static void ApplyPlatformWindowChrome(this Window window)
    {
        if (IsDesignMode)
        {
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            window.SystemDecorations = SystemDecorations.Full;
            NitroxAttached.SetUseCustomTitleBar(window, false);
        }
        else if (OperatingSystem.IsMacOS())
        {
            window.SystemDecorations = SystemDecorations.Full;
            window.ExtendClientAreaToDecorationsHint = false;
            window.ExtendClientAreaTitleBarHeightHint = -1;
            NitroxAttached.SetUseCustomTitleBar(window, false);
        }
        else if (OperatingSystem.IsWindows())
        {
            nint? windowHandle = window.TryGetPlatformHandle()?.Handle;
            if (!windowHandle.HasValue)
            {
                return;
            }

            WindowsApi.EnableDefaultWindowAnimations(windowHandle.Value, window.CanResize);
        }
    }

    public static Window? GetWindow(this Visual visual) => TopLevel.GetTopLevel(visual) as Window;
}
