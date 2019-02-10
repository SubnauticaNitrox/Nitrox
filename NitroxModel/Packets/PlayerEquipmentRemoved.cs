using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerEquipmentRemoved : Packet
    {
        public TechType TechType { get; }
        public string EquippedItemGuid { get; }

        public PlayerEquipmentRemoved(TechType techType, string equippedItemGuid)
        {
            TechType = techType;
            EquippedItemGuid = equippedItemGuid;
        }
    }
}
