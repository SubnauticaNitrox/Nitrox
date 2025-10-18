using System.ComponentModel;

namespace NitroxModel.Platforms.Discovery.Models;

public enum Platform
{
    [Description("Standalone")]
    NONE,

    [Description("Steam")]
    STEAM,

    [Description("Epic Games Store")]
    EPIC,

    [Description("Heroic Games Launcher")]
    HEROIC,

    [Description("Microsoft")]
    MICROSOFT,

    [Description("Discord")]
    DISCORD
}
