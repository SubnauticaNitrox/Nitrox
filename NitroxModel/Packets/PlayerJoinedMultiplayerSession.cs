using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoinedMultiplayerSession : Packet
    {
        public string PlayerId { get; }
        public string PlayerName { get; }
        public PlayerSettings PlayerSettings { get; }

        public PlayerJoinedMultiplayerSession(string playerId, string playerName, PlayerSettings playerSettings)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            PlayerSettings = playerSettings;
        }
    }
}
