using System;
using System.Runtime.InteropServices;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Platforms.OS.Windows;

public class WindowsApi
{
    public static void EnableDefaultWindowAnimations(IntPtr hWnd, int nIndex = -16)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IntPtr dwNewLong = new((long)(Win32Native.WS.WS_CAPTION | Win32Native.WS.WS_CLIPCHILDREN | Win32Native.WS.WS_MINIMIZEBOX | Win32Native.WS.WS_MAXIMIZEBOX | Win32Native.WS.WS_SYSMENU | Win32Native.WS.WS_SIZEBOX));
            HandleRef handle = new(null, hWnd);
            switch (IntPtr.Size)
            {
                case 8:
                    Win32Native.SetWindowLongPtr64(handle, nIndex, dwNewLong);
                    break;
                default:
                    Win32Native.SetWindowLong32(handle, nIndex, dwNewLong.ToInt32());
                    break;
            }
        }
    }
}
