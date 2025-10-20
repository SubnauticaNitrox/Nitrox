using System.ComponentModel;

namespace Nitrox.Model.Discovery.Models;

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
