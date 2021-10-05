using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Structs
{
    /// <summary>
    ///     See docs (copy paste url in browser): https://www.vergiliusproject.com/kernels/x64/Windows 10 | 2016/2104 21H1 (May 2021 Update)/_IMAGE_NT_HEADERS64
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ImageNtHeader64
    {
        /// <summary>
        ///     Expected to be "PE" string for NT systems.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] [FieldOffset(0x0)]
        public char[] Magic;

        [FieldOffset(0x18)] public OptionalHeader64 OptionalHeader;
    }
}