using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerEquipmentRemoved : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual NitroxId EquippedItemId { get; protected set; }

        public PlayerEquipmentRemoved() { }

        public PlayerEquipmentRemoved(NitroxTechType techType, NitroxId equippeditemId)
        {
            TechType = techType;
            EquippedItemId = equippeditemId;
        }
    }
}
