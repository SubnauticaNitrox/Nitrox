using System;

namespace NitroxModel.Discovery.Models;

[Flags]
public enum GameLibraries
{
    /// <summary>
    /// Steam registry
    /// </summary>
    STEAM = 1 << 0,

    /// <summary>
    /// Epic games
    /// </summary>
    EPIC = 1 << 1,

    /// <summary>
    /// Microsoft store
    /// </summary>
    MICROSOFT = 1 << 2,

    /// <summary>
    /// Discord game store
    /// </summary>
    DISCORD = 1 << 3,

    /// <summary>
    /// Local config value
    /// </summary>
    CONFIG = 1 << 4,

    /// <summary>
    /// Environment variable value
    /// </summary>
    ENVIRONMENT = 1 << 5,

    /// <summary>
    /// Current directory
    /// </summary>
    CURRENT_DIRECTORY = 1 << 6,

    /// <summary>
    /// Related to an official game platform
    /// </summary>
    PLATFORMS = STEAM | EPIC | MICROSOFT | DISCORD,

    /// <summary>
    /// Related to external provider source
    /// </summary>
    CUSTOM = CONFIG | ENVIRONMENT | CURRENT_DIRECTORY,

    /// <summary>
    /// All Nitrox supported provider
    /// </summary>
    ALL = PLATFORMS | CUSTOM
}
