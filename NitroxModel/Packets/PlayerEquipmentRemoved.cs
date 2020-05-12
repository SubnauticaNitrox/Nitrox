using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentRemoved : Packet
    {
        public DataStructures.TechType TechType { get; }
        public NitroxId EquippedItemId { get; }

        public PlayerEquipmentRemoved(DataStructures.TechType techType, NitroxId equippeditemId)
        {
            TechType = techType;
            EquippedItemId = equippeditemId;
        }
    }
}
