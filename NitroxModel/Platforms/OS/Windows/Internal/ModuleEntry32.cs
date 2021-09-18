using System;
using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct ModuleEntry32
    {
        public int dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szModule;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExePath;
    }
}
