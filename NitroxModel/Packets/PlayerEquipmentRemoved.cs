using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentRemoved : Packet, IVolatilePacket
    {
        public TechType TechType { get; }
        public NitroxId EquippedItemId { get; }

        public PlayerEquipmentRemoved(TechType techType, NitroxId equippeditemId)
        {
            TechType = techType;
            EquippedItemId = equippeditemId;
        }
    }
}
