using System;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        ALL = 0x001F0FFF,
        TERMINATE = 0x00000001,
        CREATE_THREAD = 0x00000002,
        VIRTUAL_MEMORY_OPERATION = 0x00000008,
        VIRTUAL_MEMORY_READ = 0x00000010,
        VIRTUAL_MEMORY_WRITE = 0x00000020,
        DUPLICATE_HANDLE = 0x00000040,
        CREATE_PROCESS = 0x000000080,
        SET_QUOTA = 0x00000100,
        SET_INFORMATION = 0x00000200,
        QUERY_INFORMATION = 0x00000400,
        QUERY_LIMITED_INFORMATION = 0x00001000,
        SYNCHRONIZE = 0x00100000
    }
}
