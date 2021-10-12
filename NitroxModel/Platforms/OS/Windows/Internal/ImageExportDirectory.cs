using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageExportDirectory
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;

        /// <summary>
        ///     RVA of image name.
        /// </summary>
        public uint Name;

        public uint Base;
        public uint NumberOfFunctions;
        public uint NumberOfNames;
        public uint AddressOfFunctions;
        public uint AddressOfNames;
        public uint AddressOfNameOrdinals;

        public override string ToString()
        {
            return
                $"{nameof(Characteristics)}: {Characteristics}, {nameof(TimeDateStamp)}: {TimeDateStamp}, {nameof(MajorVersion)}: {MajorVersion}, {nameof(MinorVersion)}: {MinorVersion}, {nameof(Name)}: {Name}, {nameof(Base)}: {Base}, {nameof(NumberOfFunctions)}: {NumberOfFunctions}, {nameof(NumberOfNames)}: {NumberOfNames}, {nameof(AddressOfFunctions)}: {AddressOfFunctions:X}, {nameof(AddressOfNames)}: {AddressOfNames:X}, {nameof(AddressOfNameOrdinals)}: {AddressOfNameOrdinals:X}";
        }
    }
}
