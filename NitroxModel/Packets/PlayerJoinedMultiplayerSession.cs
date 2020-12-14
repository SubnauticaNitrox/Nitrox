using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.MultiplayerSession;

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

        public override string ToString()
        {
            return $"[PlayerJoinedMultiplayerSession - PlayerContext: {PlayerContext}, EquippedTechTypes: {EquippedTechTypes?.Count}]";
        }

        public override string ToLongString()
        {
            return $"[PlayerJoinedMultiplayerSession - PlayerContext: {PlayerContext}, EquippedTechTypes: ({string.Join(", ", EquippedTechTypes)})]";
        }
    }
}
