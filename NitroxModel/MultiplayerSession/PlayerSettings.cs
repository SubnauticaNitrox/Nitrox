using System;
using NitroxModel.DataStructures.Unity;

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
    }
}
