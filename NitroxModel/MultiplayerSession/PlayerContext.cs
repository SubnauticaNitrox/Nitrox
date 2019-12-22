using System;
using NitroxModel.DataStructures;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public NitroxId PlayerId { get; }
        public string PlayerName { get; }
        public bool WasBrandNewPlayer { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerContext(string playerName, NitroxId playerId, bool wasBrandNewPlayer, PlayerSettings playerSettings)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            WasBrandNewPlayer = wasBrandNewPlayer;
            PlayerSettings = playerSettings;
        }
    }
}
