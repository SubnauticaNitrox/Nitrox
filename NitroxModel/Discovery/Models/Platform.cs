using System.ComponentModel;

namespace NitroxModel.Discovery
{
    public enum Platform
    {
        [Description("Standalone")]
        NONE,

        [Description("Pirated")]
        PIRATED,

        [Description("Epic Games Store")]
        EPIC,

        [Description("Steam")]
        STEAM,

        [Description("Microsoft")]
        MICROSOFT,

        [Description("Discord")]
        DISCORD
    }
}
