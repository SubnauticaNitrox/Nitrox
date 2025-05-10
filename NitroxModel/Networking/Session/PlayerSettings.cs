using System;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Session
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
