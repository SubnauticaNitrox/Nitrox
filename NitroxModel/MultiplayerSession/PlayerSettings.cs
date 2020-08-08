using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerSettings
    {
        public NitroxColor PlayerColor { get; }

        public PlayerSettings(NitroxColor playerColor)
        {
            PlayerColor = playerColor;
        }

        public override string ToString()
        {
            return $"[PlayerSettings - PlayerColor: {PlayerColor}]";
        }
    }
}
