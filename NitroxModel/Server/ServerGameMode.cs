using System.ComponentModel;

namespace NitroxModel.Server
{
    public enum ServerGameMode
    {
        [Description("Survival")]
        SURVIVAL,
        [Description("Creative")]
        CREATIVE,
        [Description("Freedom")]
        FREEDOM,
        [Description("Hardcore")]
        HARDCORE
    }
}
