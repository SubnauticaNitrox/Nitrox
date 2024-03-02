using System;

namespace NitroxModel.Discovery.Models;

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
    /// Epic games
    /// </summary>
    EPIC = 1 << 3,

    /// <summary>
    /// Microsoft store
    /// </summary>
    MICROSOFT = 1 << 4,

    /// <summary>
    /// Discord game store
    /// </summary>
    DISCORD = 1 << 5,

    /// <summary>
    /// Related to an official game platform
    /// </summary>
    PLATFORMS = STEAM | EPIC | MICROSOFT | DISCORD,

    /// <summary>
    /// Related to an external provider source
    /// </summary>
    CUSTOM = CONFIG | ENVIRONMENT,

    /// <summary>
    /// All Nitrox supported provider
    /// </summary>
    ALL = PLATFORMS | CUSTOM
}
