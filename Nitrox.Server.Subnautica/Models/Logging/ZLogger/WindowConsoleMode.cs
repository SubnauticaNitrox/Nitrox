using System.Runtime.InteropServices;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal static partial class WindowsConsoleMode
{
    [LibraryImport("kernel32.dll")]
    internal static partial IntPtr GetStdHandle(int nStdHandle);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetConsoleMode(IntPtr handle, out int mode);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetConsoleMode(IntPtr handle, int mode);

    internal static bool TryEnableVirtualTerminalProcessing()
    {
        IntPtr stdHandle = GetStdHandle(-11);
        return GetConsoleMode(stdHandle, out int mode) && SetConsoleMode(stdHandle, mode | 4);
    }
}
