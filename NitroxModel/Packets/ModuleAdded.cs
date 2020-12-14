using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ModuleAdded : Packet
    {
        public EquippedItemData EquippedItemData { get; }

        public ModuleAdded(EquippedItemData equippedItemData)
        {
            EquippedItemData = equippedItemData;
        }

        public override string ToString()
        {
            return $"[ModuleAdded - EquippedItemData: {EquippedItemData}]";
        }
    }
}
