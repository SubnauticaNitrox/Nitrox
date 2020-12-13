using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class PlayerEquipmentRemoved : Packet
    {
        public NitroxTechType TechType { get; }
        public NitroxId EquippedItemId { get; }

        public PlayerEquipmentRemoved(NitroxTechType techType, NitroxId equippeditemId)
        {
            TechType = techType;
            EquippedItemId = equippeditemId;
        }
    }
}
