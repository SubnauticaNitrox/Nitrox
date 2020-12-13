using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.MultiplayerSession
{
    [Serializable]
    public class PlayerSettings
    {
        public NitroxColor PlayerColor { get; }

        public PlayerSettings(NitroxColor playerColor)
        {
            PlayerColor = playerColor;
        }
    }
}
