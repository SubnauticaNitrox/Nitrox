using System;
using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    /// <summary>
    ///     PEB structure in a used by Windows in 64-bit processes.
    ///     See full structure here:
    ///     https://ntopcode.wordpress.com/2018/02/26/anatomy-of-the-process-environment-block-peb-windows-internals/
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ProcessEnvironmentBlock64
    {
        [FieldOffset(0x0)]
        public byte Reserved1;
        [FieldOffset(0x1)]
        public byte Reserved2;
        [FieldOffset(0x2)]
        public byte BeingDebugged;
        [FieldOffset(0x3)]
        public byte Reserved3;

        [FieldOffset(0x10)]
        public IntPtr ImageBaseAddress;

        /// <summary>
        ///     Ptr to structure <see cref="Windows.Internal.LdrData" />.
        /// </summary>
        [FieldOffset(0x18)]
        public IntPtr LdrData;

        [FieldOffset(0x20)]
        public IntPtr ProcessParameters;
    }
}
