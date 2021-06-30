using System;

namespace NitroxModel.DataStructures.GameLogic
{
    public enum Perms : byte
    {
        NONE,
        PLAYER,
        MODERATOR,
        ADMIN,
        CONSOLE
    }

    [Flags]
    public enum PermsFlag : byte
    {
        NONE = 0x0,
        NO_CONSOLE = 0x1
    }
}
