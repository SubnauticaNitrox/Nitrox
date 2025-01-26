using System;
using System.Runtime.InteropServices;
using static NitroxModel.Platforms.OS.Windows.Internal.Win32Native;

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

        WS dwNewLong = WS.WS_CAPTION | WS.WS_CLIPCHILDREN | WS.WS_MINIMIZEBOX | WS.WS_MAXIMIZEBOX | WS.WS_SYSMENU;
        if (canResize)
        {
            dwNewLong |= WS.WS_SIZEBOX;
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
