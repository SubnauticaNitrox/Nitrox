using System;
using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct ProcessInfo
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public Int32 ProcessId;
        public Int32 ThreadId;
    }
}
