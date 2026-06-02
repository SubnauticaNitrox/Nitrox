using System.Runtime.InteropServices;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal static class WindowsConsoleMode
{
    [DllImport("kernel32.dll")]
    internal static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GetConsoleMode(IntPtr handle, out int mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool SetConsoleMode(IntPtr handle, int mode);

    internal static bool TryEnableVirtualTerminalProcessing()
    {
        IntPtr stdHandle = GetStdHandle(-11);
        return GetConsoleMode(stdHandle, out int mode) && SetConsoleMode(stdHandle, mode | 4);
    }
}
