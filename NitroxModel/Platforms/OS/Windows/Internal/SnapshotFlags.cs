using System;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    [Flags]
    public enum SnapshotFlags : uint
    {
        HEAP_LIST = 0x00000001,
        PROCESS = 0x00000002,
        THREAD = 0x00000004,
        MODULE = 0x00000008,
        MODULE32 = 0x00000010,
        INHERIT = 0x80000000,
        ALL = 0x0000001F,
        NO_HEAPS = 0x40000000
    }
}
