using System;
using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SecurityAttributes
    {
        public int length;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
    }
}
