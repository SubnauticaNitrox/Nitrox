using System;
using System.Runtime.InteropServices;
using Nitrox.Model.Platforms.OS.Windows.Internal;
using static Nitrox.Model.Platforms.OS.Windows.Internal.Win32Native;

namespace Nitrox.Model.Platforms.OS.Windows;

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
                SetWindowLongPtr64(handle, -16, (long)dwNewLong);
                break;
            default:
                SetWindowLong32(handle, -16, (int)dwNewLong);
                break;
        }
    }

    public static void BringProcessToFront(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return;
        }
        const int SW_RESTORE = 9;
        if (IsIconic(windowHandle))
        {
            ShowWindow(windowHandle, SW_RESTORE);
        }

        SetForegroundWindow(windowHandle);
    }

    [DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr handle);
    [DllImport("User32.dll")]
    private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
    [DllImport("User32.dll")]
    private static extern bool IsIconic(IntPtr handle);
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
}
