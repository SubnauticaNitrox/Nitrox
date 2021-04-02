using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.MultiplayerSession;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoinedMultiplayerSession : Packet
    {
        public PlayerContext PlayerContext { get; }
        public Optional<NitroxId> SubRootId { get; }
        public List<NitroxTechType> EquippedTechTypes { get; }
        public List<ItemData> InventoryItems { get; }

        public PlayerJoinedMultiplayerSession(PlayerContext playerContext, Optional<NitroxId> subRootId, List<NitroxTechType> equippedTechTypes, List<ItemData> inventoryItems)
        {
            PlayerContext = playerContext;
            SubRootId = subRootId;
            EquippedTechTypes = equippedTechTypes;
            InventoryItems = inventoryItems;
        }
    }
}
