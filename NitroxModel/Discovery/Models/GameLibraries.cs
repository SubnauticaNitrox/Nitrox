using System;

namespace NitroxModel.Discovery.Models;

[Flags]
public enum GameLibraries
{
    /// <summary>
    /// Steam registry finder
    /// </summary>
    STEAM = 1 << 0,

    /// <summary>
    /// Epic games finder
    /// </summary>
    EPIC = 1 << 1,

    /// <summary>
    /// Microsoft store finder
    /// </summary>
    MICROSOFT = 1 << 2,

    /// <summary>
    /// Discord game store finder
    /// </summary>
    DISCORD = 1 << 3,

    /// <summary>
    /// Local config value finder
    /// </summary>
    CONFIG = 1 << 4,

    /// <summary>
    /// Environment variable value finder
    /// </summary>
    ENVIRONMENT = 1 << 5,

    /// <summary>
    /// Current directory finder
    /// </summary>
    CURRENT_DIRECTORY = 1 << 6,

    PLATFORMS = STEAM | EPIC | MICROSOFT | DISCORD,

    ALL = STEAM | EPIC | MICROSOFT | DISCORD | CONFIG | CONFIG | ENVIRONMENT | CURRENT_DIRECTORY
}
