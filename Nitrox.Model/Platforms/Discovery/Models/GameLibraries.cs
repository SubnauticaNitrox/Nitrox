using System;

namespace Nitrox.Model.Platforms.Discovery.Models;

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
    /// Steam
    /// </summary>
    STEAM = 1 << 2,

    /// <summary>
    /// Windows game installation in a Wine-style prefix.
    /// </summary>
    WINE = 1 << 3,

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
    CUSTOM = CONFIG | ENVIRONMENT | WINE,

    /// <summary>
    /// All Nitrox supported provider
    /// </summary>
    ALL = PLATFORMS | CUSTOM
}
