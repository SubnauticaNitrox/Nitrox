using NitroxModel.DataStructures.GameLogic;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ModuleAdded : Packet
    {
        public EquippedItemData EquippedItemData { get; }
        public bool PlayerModule { get; }

        public ModuleAdded(EquippedItemData equippedItemData, bool playerModule)
        {
            EquippedItemData = equippedItemData;
            PlayerModule = playerModule;
        }

        public override string ToString()
        {
            return $"[ModuleAdded EquippedItemData: {EquippedItemData}, PlayerModule: {PlayerModule}]";
        }
    }
}
