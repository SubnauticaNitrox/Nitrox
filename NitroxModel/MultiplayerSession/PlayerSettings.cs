using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.MultiplayerSession
{
    [ZeroFormattable]
    public class PlayerSettings
    {
        [Index(0)]
        public virtual NitroxColor PlayerColor { get; protected set; }

        public PlayerSettings() { }

        public PlayerSettings(NitroxColor playerColor)
        {
            PlayerColor = playerColor;
        }
    }
}
