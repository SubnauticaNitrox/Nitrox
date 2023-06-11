using System;

namespace NitroxModel.Discovery.Models;

[Flags]
public enum GameLibraries
{
    UNKNOWN = 0,

    /// <summary>
    /// Steam registry finder
    /// </summary>
    STEAM = 1,

    /// <summary>
    /// Epic games finder
    /// </summary>
    EPIC = 2,

    /// <summary>
    /// Microsoft store finder
    /// </summary>
    MICROSOFT = 3,

    /// <summary>
    /// Discord game store finder
    /// </summary>
    DISCORD = 4,

    /// <summary>
    /// Local config value finder
    /// </summary>
    CONFIG = 10,

    /// <summary>
    /// Environment variable value finder
    /// </summary>
    ENVIRONMENT = 11,

    /// <summary>
    /// Current directory finder
    /// </summary>
    CURRENT_DIRECTORY = 12,

    PLATFORMS = STEAM | EPIC | MICROSOFT | DISCORD,

    ALL = STEAM | EPIC | MICROSOFT | DISCORD | CONFIG | CONFIG | ENVIRONMENT | CURRENT_DIRECTORY
}
