using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerSettings
    {
        public NitroxColor PlayerColor { get; }
        public int PlayerVisibility { get; }

        public PlayerSettings(NitroxColor playerColor, int playerVisibility)
        {
            PlayerColor = playerColor;
            PlayerVisibility = playerVisibility;
        }
    }
}
