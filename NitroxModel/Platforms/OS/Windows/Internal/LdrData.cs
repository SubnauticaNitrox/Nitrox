using System;
using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LdrData
    {
        [FieldOffset(0x0)]
        public int Length;
        [FieldOffset(0x4)]
        public int Initialized;
        [FieldOffset(0x8)]
        public short SsHandle;
        [FieldOffset(0x10)]
        public IntPtr InLoadOrderModuleList;
        [FieldOffset(0x20)]
        public IntPtr InMemoryOrderModuleList;
        [FieldOffset(0x30)]
        public IntPtr InInitializationOrderModuleList;
    }
}
