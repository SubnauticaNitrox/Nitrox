using System;
using UnityEngine;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerSettings
    {
        public Color PlayerColor { get; }

        public PlayerSettings(Color32 playerColor)
        {
            PlayerColor = playerColor;
        }
    }
}
