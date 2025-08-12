using System.ComponentModel;

namespace NitroxModel.Discovery.Models;

public enum Platform
{
    [Description("Standalone")]
    NONE,

    [Description("Epic Games Store")]
    EPIC,

    [Description("Steam")]
    STEAM,

    [Description("Microsoft")]
    MICROSOFT,

    [Description("Discord")]
    DISCORD
}
