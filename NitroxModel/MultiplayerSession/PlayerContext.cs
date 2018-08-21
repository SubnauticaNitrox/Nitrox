using System;

namespace NitroxModel.MultiplayerSession
{
    [Serializable]
    public class PlayerContext
    {
        public ushort PlayerId { get; }
        public string PlayerName { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerContext(string playerName, ushort playerId, PlayerSettings playerSettings)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            PlayerSettings = playerSettings;
        }
    }
}
