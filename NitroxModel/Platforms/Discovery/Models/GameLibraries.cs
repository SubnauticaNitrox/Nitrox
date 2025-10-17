using System;

namespace NitroxModel.Platforms.Discovery.Models;

[Flags]
public enum GameLibraries
{
    /// <summary>
    /// Local config value
    /// </summary>
    CONFIG = 1 << 0,

    /// <summary>
    /// Environment variable value
    /// </summary>
    ENVIRONMENT = 1 << 1,

    /// <summary>
    /// Command line argument variable value
    /// </summary>
    CMD_LINE_ARGS = 1 << 2,

    /// <summary>
    /// Steam
    /// </summary>
    STEAM = 1 << 3,

    /// <summary>
    /// Epic games
    /// </summary>
    EPIC = 1 << 4,

    /// <summary>
    /// Heroic Games Launcher
    /// </summary>
    HEROIC = 1 << 5,

    /// <summary>
    /// Microsoft store
    /// </summary>
    MICROSOFT = 1 << 6,

    /// <summary>
    /// Discord game store
    /// </summary>
    DISCORD = 1 << 7,

    /// <summary>
    /// Related to an official game platform
    /// </summary>
    PLATFORMS = STEAM | EPIC | HEROIC | MICROSOFT | DISCORD,

    /// <summary>
    /// Related to an external provider source
    /// </summary>
    CUSTOM = CONFIG | ENVIRONMENT | CMD_LINE_ARGS,

    /// <summary>
    /// All Nitrox supported provider
    /// </summary>
    ALL = PLATFORMS | CUSTOM
}
