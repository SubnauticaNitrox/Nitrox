using System.ComponentModel;

namespace NitroxModel.Discovery.Models;

public enum Platform
{
    [Description("Unknown")]
    NONE = 0,

    [Description("Steam")]
    STEAM = 1 << 0,

    [Description("Epic Games Store")]
    EPIC = 1 << 1,

    [Description("Microsoft")]
    MICROSOFT = 1 << 2,

    [Description("Discord")]
    DISCORD = 1 << 3,

    [Description("Pirated")]
    PIRATED = 255
}
