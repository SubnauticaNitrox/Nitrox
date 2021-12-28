using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerJoinedMultiplayerSession : Packet
    {
        [Index(0)]
        public virtual PlayerContext PlayerContext { get; protected set; }
        [Index(1)]
        public virtual Optional<NitroxId> SubRootId { get; protected set; }
        [Index(2)]
        public virtual List<NitroxTechType> EquippedTechTypes { get; protected set; }
        [Index(3)]
        public virtual List<ItemData> InventoryItems { get; protected set; }

        public PlayerJoinedMultiplayerSession() { }

        public PlayerJoinedMultiplayerSession(PlayerContext playerContext, Optional<NitroxId> subRootId, List<NitroxTechType> equippedTechTypes, List<ItemData> inventoryItems)
        {
            PlayerContext = playerContext;
            SubRootId = subRootId;
            EquippedTechTypes = equippedTechTypes;
            InventoryItems = inventoryItems;
        }
    }
}
