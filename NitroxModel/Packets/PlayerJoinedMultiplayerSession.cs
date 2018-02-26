using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoinedMultiplayerSession : Packet
    {
        public PlayerContext PlayerContext { get; }

        public PlayerJoinedMultiplayerSession(PlayerContext playerContext)
        {
            PlayerContext = playerContext;
        }
    }
}
