using System;
using System.Runtime.InteropServices;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Platforms.OS.Windows;

public class WindowsApi
{
    /// <summary>
    ///     Applies default OS animations to the window handle.
    /// </summary>
    /// <remarks>
    ///     Note on Windows OS: it will force enable resizing of a Window if <see cref="canResize"/> is true. Make sure to set it correctly.
    /// </remarks>
    public static void EnableDefaultWindowAnimations(nint windowHandle, bool canResize)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        Win32Native.WS dwNewLong = Win32Native.WS.WS_CAPTION | Win32Native.WS.WS_CLIPCHILDREN | Win32Native.WS.WS_MINIMIZEBOX | Win32Native.WS.WS_MAXIMIZEBOX | Win32Native.WS.WS_SYSMENU;
        if (canResize)
        {
            dwNewLong |= Win32Native.WS.WS_SIZEBOX;
        }

        HandleRef handle = new(null, windowHandle);
        switch (IntPtr.Size)
        {
            case 8:
                Win32Native.SetWindowLongPtr64(handle, -16, (long)dwNewLong);
                break;
            default:
                Win32Native.SetWindowLong32(handle, -16, (int)dwNewLong);
                break;
        }
    }
}
