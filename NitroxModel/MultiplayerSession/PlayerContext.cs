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
        public Guid AuthToken { get; }

        public PlayerContext(string playerName, ushort playerId, bool wasBrandNewPlayer, PlayerSettings playerSettings, Guid authToken)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            WasBrandNewPlayer = wasBrandNewPlayer;
            PlayerSettings = playerSettings;
            AuthToken = authToken;
        }
    }
}
