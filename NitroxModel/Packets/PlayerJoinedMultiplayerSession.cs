using System;
using System.Collections.Generic;
using NitroxModel.MultiplayerSession;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoinedMultiplayerSession : Packet
    {
        public PlayerContext PlayerContext { get; }
        public List<TechType> EquippedTechTypes { get; }

        public PlayerJoinedMultiplayerSession(PlayerContext playerContext, List<TechType> equippedTechTypes)
        {
            PlayerContext = playerContext;
            EquippedTechTypes = equippedTechTypes;
        }
    }
}
