using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialRemotePlayerData
    {
        public PlayerContext PlayerContext { get; set; }        
        public NitroxVector3 Position { get; set; }
        public Optional<NitroxId> SubRootId { get; }
        public List<TechType> EquippedTechTypes { get; }

        public InitialRemotePlayerData()
        {
            // Constructor for serialization
        }

        public InitialRemotePlayerData(PlayerContext playerContext, NitroxVector3 position, Optional<NitroxId> subRootId, List<TechType> equippedTechTypes)
        {
            PlayerContext = playerContext;
            Position = position;
            SubRootId = subRootId;
            EquippedTechTypes = equippedTechTypes;
        }
    }
}
