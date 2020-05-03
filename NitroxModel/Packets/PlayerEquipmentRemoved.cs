using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentRemoved : Packet
    {
        public PlayerEquipmentRemoved(DTO.TechType techType, DTO.NitroxId equippeditemId)
        {
            TechType = techType;
            EquippedItemId = equippeditemId;
        }

        public DTO.TechType TechType { get; }
        public DTO.NitroxId EquippedItemId { get; }
    }
}
