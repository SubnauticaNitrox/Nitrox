using System.Runtime.InteropServices;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageDosHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public char[] e_magic_byte; // Magic number

        /// <summary>
        ///     Bytes on last page of file
        /// </summary>
        public ushort e_cblp;

        /// <summary>
        ///     Pages in file
        /// </summary>
        public ushort e_cp;

        public ushort e_crlc;

        /// <summary>
        ///     Size of header in paragraphs
        /// </summary>
        public ushort e_cparhdr;

        public ushort e_minalloc;
        public ushort e_maxalloc;
        public ushort e_ss;
        public ushort e_sp;

        /// <summary>
        ///     Checksum
        /// </summary>
        public ushort e_csum;

        /// <summary>
        ///     Initial IP value
        /// </summary>
        public ushort e_ip; // 

        /// <summary>
        ///     Initial (relative) CS value
        /// </summary>
        public ushort e_cs;

        /// <summary>
        ///     File address of relocation table
        /// </summary>
        public ushort e_lfarlc;

        /// <summary>
        ///     Overlay number
        /// </summary>
        public ushort e_ovno;

        /// <summary>
        ///     Reserved words
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] e_res1;
        
        public ushort e_oemid;
        public ushort e_oeminfo;
        
        /// <summary>
        ///     Reserved words
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public ushort[] e_res2;
        
        /// <summary>
        ///     Image base offset to address of PE header.
        /// </summary>
        public int e_lfanew;
    }
}
