using System;

namespace NitroxModel.DataStructures.GameLogic
{
    public enum Perms : byte
    {
        /// <summary>
        /// No permissions
        /// </summary>
        NONE,
        /// <summary>
        /// Default player permission, cannot use cheat and have access to basic server commands (e.g: help, list, whisper, whois, ...)
        /// </summary>
        PLAYER,
        /// <summary>
        /// Player that can manage other players in game. Can use vanilla cheat commands and some advanced server commands (e.g: mute, kick, broadcast, ...)
        /// </summary>
        MODERATOR,
        /// <summary>
        /// Server administrator, can manage server settings and players. Can use vanilla cheat commands and all server commands (e.g: op, promote, server settings, ...)
        /// </summary>
        ADMIN,
        /// <summary>
        /// All permissions
        /// </summary>
        CONSOLE
    }

    [Flags]
    public enum PermsFlag : byte
    {
        NONE = 0x0,
        NO_CONSOLE = 0x1
    }
}
