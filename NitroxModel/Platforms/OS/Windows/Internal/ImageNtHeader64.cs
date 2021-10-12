using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    /// <summary>
    /// See: https://www.vergiliusproject.com/kernels/x64/Windows%2010%20|%202016/2009%2020H2%20(October%202020%20Update)/_IMAGE_NT_HEADERS64
    /// And MSDN: 
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ImageNtHeader64
    {
        /// <summary>
        ///     Expected to be "PE" string for NT systems.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        [FieldOffset(0x0)]
        public char[] Magic;

        [FieldOffset(0x18)]
        public OptionalHeader64 OptionalHeader;
    }
}
