using System;
using Nitrox.Model.DataStructures.Unity;

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

        public override string ToString()
        {
            return $"[{nameof(PlayerSettings)}: PlayerColor: {PlayerColor}]";
        }
    }
}
