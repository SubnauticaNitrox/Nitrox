using System.ComponentModel;

namespace NitroxModel.Server
{
    public enum ServerGameMode
    {
        [Description("Survival")]
        SURVIVAL = 0,
        [Description("Freedom")]
        FREEDOM = 2,
        [Description("Hardcore")]
        HARDCORE = 257,
        [Description("Creative")]
        CREATIVE = 1790,
    }
}
