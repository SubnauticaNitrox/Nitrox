using System;
using NitroxModel.DataStructures;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerSettings
    {
        public PlayerSettings(Color playerColor)
        {
            PlayerColor = playerColor;
        }

        public Color PlayerColor { get; }
    }
}
