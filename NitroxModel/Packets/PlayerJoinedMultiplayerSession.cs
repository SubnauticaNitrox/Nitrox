using System;
using System.Collections.Generic;
using NitroxModel.MultiplayerSession;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoinedMultiplayerSession : Packet
    {
        public PlayerContext PlayerContext { get; }
        public List<NitroxTechType> EquippedTechTypes { get; }

        public PlayerJoinedMultiplayerSession(PlayerContext playerContext, List<NitroxTechType> equippedTechTypes)
        {
            PlayerContext = playerContext;
            EquippedTechTypes = equippedTechTypes;
        }
    }
}
