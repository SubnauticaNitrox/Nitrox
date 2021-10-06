using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    /// <summary>
    ///     Structs from (copy paste url in browser to visit):
    ///     <a
    ///         href="https://www.vergiliusproject.com/kernels/x64/Windows 10 |%202016%2F2104 21H1 (May 2021 Update)/_IMAGE_OPTIONAL_HEADER64">
    ///         _IMAGE_OPTIONAL_HEADER64
    ///     </a>
    ///     Also see:
    ///     <a href="https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#optional-header-image-only">MSDN: PE Header</a>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct OptionalHeader64
    {
        /// <summary>
        ///     Expected to be "PE" string for NT systems.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public char[] Magic;

        public byte MajorLinkerVersion;
        public byte MinorLinkerVersion;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData;
        public uint AddressOfEntryPoint;
        public uint BaseOfCode;
        public ulong ImageBase;
        public uint SectionAlignment;
        public uint FileAlignment;
        public ushort MajorOperatingSystemVersion;
        public ushort MinorOperatingSystemVersion;
        public ushort MajorImageVersion;
        public ushort MinorImageVersion;
        public ushort MajorSubsystemVersion;
        public ushort MinorSubsystemVersion;
        public uint Win32VersionValue;
        public uint SizeOfImage;
        public uint SizeOfHeaders;
        public uint CheckSum;
        public ushort Subsystem;
        public ushort DllCharacteristics;
        public ulong SizeOfStackReserve;
        public ulong SizeOfStackCommit;
        public ulong SizeOfHeapReserve;
        public ulong SizeOfHeapCommit;
        public uint LoaderFlags;
        public uint NumberOfRvaAndSizes;
        public ImageDataDirectory ExportTable;
        public ImageDataDirectory ImportTable;
        public ImageDataDirectory ResourceTable;
        public ImageDataDirectory ExceptionTable;
        public ImageDataDirectory CertificateTable;
        public ImageDataDirectory BaseRelocationTable;
        public ImageDataDirectory Debug;
        public ImageDataDirectory Architecture;
        public ImageDataDirectory GlobalPtr;
        public ImageDataDirectory TLSTable;
        public ImageDataDirectory LoadConfigTable;
        public ImageDataDirectory BoundImport;
        public ImageDataDirectory IAT;
        public ImageDataDirectory DelayImportDescriptor;
        public ImageDataDirectory CLRRuntimeHeader;
        public ImageDataDirectory Reserved;

        public override string ToString()
        {
            return
                $"{nameof(Magic)}: {new string(Magic)}, {nameof(MajorLinkerVersion)}: {MajorLinkerVersion}, {nameof(MinorLinkerVersion)}: {MinorLinkerVersion}, {nameof(SizeOfCode)}: {SizeOfCode}, {nameof(SizeOfInitializedData)}: {SizeOfInitializedData}, {nameof(SizeOfUninitializedData)}: {SizeOfUninitializedData}, {nameof(AddressOfEntryPoint)}: {AddressOfEntryPoint}, {nameof(BaseOfCode)}: {BaseOfCode}, {nameof(ImageBase)}: {ImageBase}, {nameof(SectionAlignment)}: {SectionAlignment}, {nameof(FileAlignment)}: {FileAlignment}, {nameof(MajorOperatingSystemVersion)}: {MajorOperatingSystemVersion}, {nameof(MinorOperatingSystemVersion)}: {MinorOperatingSystemVersion}, {nameof(MajorImageVersion)}: {MajorImageVersion}, {nameof(MinorImageVersion)}: {MinorImageVersion}, {nameof(MajorSubsystemVersion)}: {MajorSubsystemVersion}, {nameof(MinorSubsystemVersion)}: {MinorSubsystemVersion}, {nameof(Win32VersionValue)}: {Win32VersionValue}, {nameof(SizeOfImage)}: {SizeOfImage}, {nameof(SizeOfHeaders)}: {SizeOfHeaders}, {nameof(CheckSum)}: {CheckSum}, {nameof(Subsystem)}: {Subsystem}, {nameof(DllCharacteristics)}: {DllCharacteristics}, {nameof(SizeOfStackReserve)}: {SizeOfStackReserve}, {nameof(SizeOfStackCommit)}: {SizeOfStackCommit}, {nameof(SizeOfHeapReserve)}: {SizeOfHeapReserve}, {nameof(SizeOfHeapCommit)}: {SizeOfHeapCommit}, {nameof(LoaderFlags)}: {LoaderFlags}, {nameof(NumberOfRvaAndSizes)}: {NumberOfRvaAndSizes}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDataDirectory
    {
        /// <summary>
        ///     Location of the data in this directory. Add this value to the base address of the image to get the real address.
        /// </summary>
        public uint VirtualAddress;

        public uint Size;
    }
}
