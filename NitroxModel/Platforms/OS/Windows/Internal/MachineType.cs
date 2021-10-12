namespace NitroxModel.Platforms.OS.Windows.Internal
{
    /// <summary>
    ///     See: <a href="https://docs.microsoft.com/en-us/windows/win32/debug/pe-format#machine-types">MSDN</a>
    /// </summary>
    public enum MachineType : ushort
    {
        UNKNOWN = 0,
        AM33 = 0x1d3,
        AMD64 = 0x8664,
        ARM = 0x1c0,
        ARM64 = 0xaa64,
        ARMNT = 0xc4,
        EBC = 0xebc, // EFI byte code
        I386 = 0x14c,
        IA64 = 0x200,
        M32R = 0x9041,
        MIPS16 = 0x266,
        POWER_PC = 0x1f1
    }
}
