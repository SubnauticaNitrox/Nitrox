using System;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public string PlayerName { get; }
        public PlayerSettings PlayerSettings { get; }
        public ulong LPlayerId { get; }

        public PlayerContext(string playerName, ulong playerId, PlayerSettings playerSettings)
        {
            LPlayerId = playerId;
            PlayerName = playerName;
            PlayerSettings = playerSettings;
        }
    }
}
