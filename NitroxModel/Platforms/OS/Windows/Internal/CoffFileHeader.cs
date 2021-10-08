using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    /// <summary>
    ///     See:
    ///     <a href="https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#coff-file-header-object-and-image">MSDN</a>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CoffFileHeader
    {
        /// <summary>
        ///     Machine type that is targeted by this image.
        /// </summary>
        public MachineType Machine;

        public ushort NumberOfSections;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable;
        public uint NumberOfSymbols;
        public ushort SizeOfOptionalHeader;
        public ushort Characteristics;
    }
}
