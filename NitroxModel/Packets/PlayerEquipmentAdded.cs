using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerEquipmentAdded : Packet
    {
        [Index(0)]
        public virtual NitroxTechType TechType { get; protected set; }
        [Index(1)]
        public virtual EquippedItemData EquippedItem { get; protected set; }

        private PlayerEquipmentAdded() { }

        public PlayerEquipmentAdded(NitroxTechType techType, EquippedItemData equippedItem)
        {
            TechType = techType;
            EquippedItem = equippedItem;
        }
    }
}
