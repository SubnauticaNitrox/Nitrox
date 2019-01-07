using System;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public ushort PlayerId { get; }
        public string PlayerName { get; }
        public bool WasBrandNewPlayer { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerContext(string playerName, ushort playerId, bool wasBrandNewPlayer, PlayerSettings playerSettings)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            WasBrandNewPlayer = wasBrandNewPlayer;
            PlayerSettings = playerSettings;
        }
    }
}
